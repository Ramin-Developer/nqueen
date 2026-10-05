using System.Diagnostics;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

// GPU spike: Unique CountOnly via the symmetry-class (Takaken) counter on ILGPU/CUDA.
// The CPU expands the search to a fixed split depth; each GPU thread finishes one prefix with
// an iterative DFS and classifies leaves as COUNT2/4/8, mirroring BitmaskSolver.SymmetryClass.

int[] sizes = args.Length > 0 ? [.. args.Select(int.Parse)] : [16, 17, 18];
long[] expected = [0, 1, 0, 0, 1, 2, 1, 6, 12, 46, 92, 341, 1787, 9233, 45752, 285053,
    1846955, 11977939, 83263591, 621012754, 4878666808];

using var context = Context.Create(b => b.Cuda());
var device = context.GetPreferredDevice(preferCPU: false);
using var accelerator = device.CreateAccelerator(context);
Console.WriteLine($"Device: {accelerator.Name} ({accelerator.AcceleratorType})");

var kernel = accelerator.LoadAutoGroupedKernel<
    Index1D, ArrayView<int>, ArrayView<ulong>, ArrayView<ulong>, ArrayView<long>, int, int, int>(
    GpuCounter.Kernel);

foreach (int n in sizes)
{
    GpuCounter.Run(accelerator, kernel, n); // warm-up (PTX compile)
    var times = new List<double>();
    long unique = 0;
    int tasks = 0;
    for (int i = 0; i < 3; i++)
    {
        var sw = Stopwatch.StartNew();
        (unique, tasks) = GpuCounter.Run(accelerator, kernel, n);
        sw.Stop();
        times.Add(sw.Elapsed.TotalMilliseconds);
    }

    string ok = n < expected.Length && unique == expected[n] ? "OK" : "MISMATCH";
    Console.WriteLine(
        $"N={n}: unique={unique:N0} [{ok}] tasks={tasks:N0} " +
        $"min={times.Min():N1} ms avg={times.Average():N1} ms");
}

internal static class GpuCounter
{
    private const int MaxN = 32;
    private const int BatchSize = 1 << 15;

    public static (long Unique, int Tasks) Run(
        Accelerator acc,
        Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<ulong>, ArrayView<ulong>, ArrayView<long>, int, int, int> kernel,
        int n)
    {
        int depth = Math.Min(7, n - 3);
        var gen = new TaskGenerator(n, depth);
        gen.Generate();
        int count = gen.Meta.Count / 3;

        using var meta = acc.Allocate1D(gen.Meta.ToArray());
        using var state = acc.Allocate1D(gen.State.ToArray());
        using var prefix = acc.Allocate1D(gen.Prefix.ToArray());
        using var output = acc.Allocate1D<long>(count * 3L);
        output.MemSetToZero();

        // Batched launches keep each kernel below the Windows TDR watchdog limit.
        for (int offset = 0; offset < count; offset += BatchSize)
        {
            int len = Math.Min(BatchSize, count - offset);
            kernel(acc.DefaultStream, len, meta.View, state.View, prefix.View, output.View, n, depth, offset);
            acc.Synchronize();
        }

        long[] res = output.GetAsArray1D();
        long c2 = 0, c4 = 0, c8 = 0;
        for (int i = 0; i < count; i++)
        {
            c2 += res[3 * i];
            c4 += res[3 * i + 1];
            c8 += res[3 * i + 2];
        }

        return (c2 + c4 + c8, count);
    }

    public static void Kernel(
        Index1D index, ArrayView<int> meta, ArrayView<ulong> state, ArrayView<ulong> prefix,
        ArrayView<long> output, int n, int depth, int offset)
    {
        int t = index + offset;
        bool corner = meta[3 * t] != 0;
        int b1 = meta[3 * t + 1];
        int b2 = meta[3 * t + 2];
        ulong l = state[3 * t], d = state[3 * t + 1], r = state[3 * t + 2];

        int sizeE = n - 1;
        ulong mask = (1UL << n) - 1UL;
        ulong topBit = 1UL << sizeE;

        var board = LocalMemory.Allocate1D<ulong>(MaxN);
        var sl = LocalMemory.Allocate1D<ulong>(MaxN);
        var sd = LocalMemory.Allocate1D<ulong>(MaxN);
        var sr = LocalMemory.Allocate1D<ulong>(MaxN);
        var sb = LocalMemory.Allocate1D<ulong>(MaxN);
        for (int i = 0; i < depth; i++)
            board[i] = prefix[t * depth + i];

        long c2 = 0, c4 = 0, c8 = 0;
        ulong side = topBit | 1UL;
        ulong lastMask = side, endBit = topBit >> 1;
        for (int i = 0; i < b1 - 1; i++)
        {
            lastMask |= (lastMask >> 1) | (lastMask << 1);
            endBit >>= 1;
        }

        int y0 = depth, y = depth;
        while (true)
        {
            bool push = true;
            ulong bitmap = mask & ~(l | d | r);
            if (y == sizeE)
            {
                push = false;
                if (corner)
                {
                    if (bitmap != 0) c8++;
                }
                else if (bitmap != 0 && (bitmap & lastMask) == 0)
                {
                    board[y] = bitmap;
                    int k = Classify(board, sizeE, b1, b2, topBit, endBit);
                    if (k == 2) c2++;
                    else if (k == 4) c4++;
                    else if (k == 8) c8++;
                }
            }
            else if (corner)
            {
                if (y < b1) bitmap &= ~2UL;
            }
            else if (y < b1)
            {
                bitmap &= ~side;
            }
            else if (y == b2)
            {
                if ((d & side) == 0) push = false;
                else if ((d & side) != side) bitmap &= side;
            }

            if (push)
            {
                sl[y] = l; sd[y] = d; sr[y] = r; sb[y] = bitmap;
            }
            else
            {
                y--;
            }

            while (y >= y0 && sb[y] == 0) y--;
            if (y < y0) break;

            ulong bm = sb[y];
            ulong bit = bm & (ulong)-(long)bm;
            sb[y] = bm ^ bit;
            board[y] = bit;
            l = (sl[y] | bit) << 1;
            d = sd[y] | bit;
            r = (sr[y] | bit) >> 1;
            y++;
        }

        output[3 * t] = c2;
        output[3 * t + 1] = c4;
        output[3 * t + 2] = c8;
    }

    private static int Classify(
        ArrayView<ulong> board, int sizeE, int b1, int b2, ulong topBit, ulong endBit)
    {
        int own, you;
        ulong bit, ptr;

        if (board[b2] == 1UL)
        {
            for (ptr = 2UL, own = 1; own <= sizeE; own++, ptr <<= 1)
            {
                bit = 1UL;
                for (you = sizeE; board[you] != ptr && board[own] >= bit; you--)
                    bit <<= 1;
                if (board[own] > bit) return 0;
                if (board[own] < bit) break;
            }
            if (own > sizeE) return 2;
        }

        if (board[sizeE] == endBit)
        {
            for (you = sizeE - 1, own = 1; own <= sizeE; own++, you--)
            {
                bit = 1UL;
                for (ptr = topBit; ptr != board[you] && board[own] >= bit; ptr >>= 1)
                    bit <<= 1;
                if (board[own] > bit) return 0;
                if (board[own] < bit) break;
            }
            if (own > sizeE) return 4;
        }

        if (board[b1] == topBit)
        {
            for (ptr = topBit >> 1, own = 1; own <= sizeE; own++, ptr >>= 1)
            {
                bit = 1UL;
                for (you = 0; board[you] != ptr && board[own] >= bit; you++)
                    bit <<= 1;
                if (board[own] > bit) return 0;
                if (board[own] < bit) break;
            }
        }

        return 8;
    }
}

// Expands corner/non-corner roots to the split depth using the same pruning rules as
// BitmaskSolver.SymmetryClassCounter (Backtrack1/Backtrack2).
internal sealed class TaskGenerator(int n, int depth)
{
    private readonly int _sizeE = n - 1;
    private readonly ulong _mask = (1UL << n) - 1UL;
    private readonly ulong _topBit = 1UL << (n - 1);
    private readonly ulong[] _board = new ulong[n];
    private bool _corner;
    private int _b1, _b2;
    private ulong _side;

    public List<int> Meta { get; } = [];
    public List<ulong> State { get; } = [];
    public List<ulong> Prefix { get; } = [];

    public void Generate()
    {
        _corner = true;
        for (int b1 = 2; b1 < _sizeE; b1++)
        {
            _b1 = _b2 = b1;
            ulong bit = 1UL << b1;
            _board[0] = 1UL;
            _board[1] = bit;
            Corner(2, (2UL | bit) << 1, 1UL | bit, bit >> 1);
        }

        _corner = false;
        _side = _topBit | 1UL;
        for (int b1 = 1, b2 = n - 2; b1 < b2; b1++, b2--)
        {
            _b1 = b1;
            _b2 = b2;
            ulong bit = 1UL << b1;
            _board[0] = bit;
            NonCorner(1, bit << 1, bit, bit >> 1);
        }
    }

    private void Emit(int y, ulong l, ulong d, ulong r)
    {
        Meta.Add(_corner ? 1 : 0);
        Meta.Add(_b1);
        Meta.Add(_b2);
        State.Add(l);
        State.Add(d);
        State.Add(r);
        for (int i = 0; i < depth; i++) Prefix.Add(i < y ? _board[i] : 0UL);
    }

    private void Corner(int y, ulong l, ulong d, ulong r)
    {
        if (y == depth)
        {
            Emit(y, l, d, r);
            return;
        }

        ulong bitmap = _mask & ~(l | d | r);
        if (y < _b1) bitmap &= ~2UL;
        while (bitmap != 0)
        {
            ulong bit = bitmap & (ulong)-(long)bitmap;
            bitmap ^= bit;
            _board[y] = bit;
            Corner(y + 1, (l | bit) << 1, d | bit, (r | bit) >> 1);
        }
    }

    private void NonCorner(int y, ulong l, ulong d, ulong r)
    {
        if (y == depth)
        {
            Emit(y, l, d, r);
            return;
        }

        ulong bitmap = _mask & ~(l | d | r);
        if (y < _b1)
        {
            bitmap &= ~_side;
        }
        else if (y == _b2)
        {
            if ((d & _side) == 0) return;
            if ((d & _side) != _side) bitmap &= _side;
        }

        while (bitmap != 0)
        {
            ulong bit = bitmap & (ulong)-(long)bitmap;
            bitmap ^= bit;
            _board[y] = bit;
            NonCorner(y + 1, (l | bit) << 1, d | bit, (r | bit) >> 1);
        }
    }
}
