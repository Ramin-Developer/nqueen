namespace NQueen.Kernel.Solvers;

public partial class BitmaskSolver
{
    // Symmetry-class (Takaken) unique counter. Instead of enumerating every solution and testing
    // all eight symmetries at each leaf, the search is split into:
    //   * corner roots (queen in row 0 at column 0): every such solution is in a class of 8;
    //   * non-corner roots: boundary masks prune non-canonical branches during the search, and the
    //     few surviving leaves get a short rotation check classifying them as COUNT2/4/8.
    // Unique = C2 + C4 + C8. Reliable for N >= 5.
    private ulong CountUniqueSymmetryClass(int n)
    {
        var items = BuildSymmetryClassItems(n);
        long c2 = 0, c4 = 0, c8 = 0;
        var po = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        var partitioner = Partitioner.Create(items, EnumerablePartitionerOptions.NoBuffering);

        Parallel.ForEach(
            partitioner,
            po,
            localInit: () => new SymmetryClassCounter(n),
            body: (item, _, local) =>
            {
                if (!IsCancellationRequested)
                    local.Run(item, this);
                return local;
            },
            localFinally: local =>
            {
                Interlocked.Add(ref c2, local.Count2);
                Interlocked.Add(ref c4, local.Count4);
                Interlocked.Add(ref c8, local.Count8);
            });

        return (ulong)(c2 + c4 + c8);
    }

    // Splits each root (corner bound1 / non-corner bound1) one row deeper so the parallel loop
    // gets ~N^2 balanced items instead of ~N coarse ones. Row-level pruning for the split row is
    // applied by SymmetryClassCounter.Run exactly as the sequential algorithm would.
    private static SymmetryClassItem[] BuildSymmetryClassItems(int n)
    {
        var list = new List<SymmetryClassItem>(n * n);
        int sizeE = n - 1;
        ulong mask = (1UL << n) - 1UL;

        for (int b1 = 2; b1 < sizeE; b1++)
        {
            ulong bit = 1UL << b1;
            ulong left = (2UL | bit) << 1, down = 1UL | bit, right = bit >> 1;
            ulong bitmap = mask & ~(left | down | right);
            if (2 < b1) bitmap &= ~2UL;
            while (bitmap != 0)
            {
                ulong b = bitmap & (ulong)-(long)bitmap;
                bitmap ^= b;
                list.Add(new SymmetryClassItem(Corner: true, b1, b1, b));
            }
        }

        for (int b1 = 1, b2 = n - 2; b1 < b2; b1++, b2--)
        {
            ulong bit = 1UL << b1;
            ulong left = bit << 1, down = bit, right = bit >> 1;
            ulong bitmap = mask & ~(left | down | right);
            ulong sideMask = (1UL << sizeE) | 1UL;
            if (1 < b1) bitmap &= ~sideMask;
            while (bitmap != 0)
            {
                ulong b = bitmap & (ulong)-(long)bitmap;
                bitmap ^= b;
                list.Add(new SymmetryClassItem(Corner: false, b1, b2, b));
            }
        }

        return [.. list];
    }

    private readonly record struct SymmetryClassItem(bool Corner, int Bound1, int Bound2, ulong SecondBit);

    private sealed class SymmetryClassCounter(int n)
    {
        private readonly ulong[] _board = new ulong[n];
        private readonly int _sizeE = n - 1;
        private readonly ulong _topBit = 1UL << (n - 1);
        private readonly ulong _mask = (1UL << n) - 1UL;
        private int _bound1, _bound2;
        private ulong _sideMask, _lastMask, _endBit;
        private BitmaskSolver? _owner;

        public long Count2, Count4, Count8;

        public void Run(SymmetryClassItem item, BitmaskSolver owner)
        {
            _owner = owner;
            _bound1 = item.Bound1;
            _bound2 = item.Bound2;
            ulong b = item.SecondBit;

            if (item.Corner)
            {
                ulong bit = 1UL << _bound1;
                _board[0] = 1UL;
                _board[1] = bit;
                ulong left = (2UL | bit) << 1, down = 1UL | bit, right = bit >> 1;
                _board[2] = b;
                Backtrack1(3, (left | b) << 1, down | b, (right | b) >> 1);
                return;
            }

            // Non-corner state for this bound1 (lastMask/endBit advance once per previous bound1).
            int k = _bound1 - 1;
            _sideMask = _topBit | 1UL;
            _lastMask = _sideMask;
            _endBit = _topBit >> 1;
            for (int i = 0; i < k; i++)
            {
                _lastMask |= (_lastMask >> 1) | (_lastMask << 1);
                _endBit >>= 1;
            }

            ulong rootBit = 1UL << _bound1;
            _board[0] = rootBit;
            _board[1] = b;
            Backtrack2(2, ((rootBit << 1) | b) << 1, rootBit | b, ((rootBit >> 1) | b) >> 1);
        }

        private void Backtrack1(int y, ulong left, ulong down, ulong right)
        {
            if (y == 5 && _owner!.IsCancellationRequested) return;
            ulong bitmap = _mask & ~(left | down | right);
            if (y == _sizeE)
            {
                if (bitmap != 0)
                {
                    _board[y] = bitmap;
                    Count8++;
                }
                return;
            }

            if (y < _bound1) bitmap &= ~2UL;
            while (bitmap != 0)
            {
                ulong bit = bitmap & (ulong)-(long)bitmap;
                bitmap ^= bit;
                _board[y] = bit;
                Backtrack1(y + 1, (left | bit) << 1, down | bit, (right | bit) >> 1);
            }
        }

        private void Backtrack2(int y, ulong left, ulong down, ulong right)
        {
            if (y == 5 && _owner!.IsCancellationRequested) return;
            ulong bitmap = _mask & ~(left | down | right);
            if (y == _sizeE)
            {
                if (bitmap != 0 && (bitmap & _lastMask) == 0)
                {
                    _board[y] = bitmap;
                    Check();
                }
                return;
            }

            if (y < _bound1)
            {
                bitmap &= ~_sideMask;
            }
            else if (y == _bound2)
            {
                if ((down & _sideMask) == 0) return;
                if ((down & _sideMask) != _sideMask) bitmap &= _sideMask;
            }

            while (bitmap != 0)
            {
                ulong bit = bitmap & (ulong)-(long)bitmap;
                bitmap ^= bit;
                _board[y] = bit;
                Backtrack2(y + 1, (left | bit) << 1, down | bit, (right | bit) >> 1);
            }
        }

        private void Check()
        {
            ulong[] board = _board;
            int sizeE = _sizeE;
            int own, you;
            ulong bit, ptr;

            // 90-degree rotation.
            if (board[_bound2] == 1UL)
            {
                for (ptr = 2UL, own = 1; own <= sizeE; own++, ptr <<= 1)
                {
                    bit = 1UL;
                    for (you = sizeE; board[you] != ptr && board[own] >= bit; you--)
                        bit <<= 1;
                    if (board[own] > bit) return;
                    if (board[own] < bit) break;
                }
                if (own > sizeE)
                {
                    Count2++;
                    return;
                }
            }

            // 180-degree rotation.
            if (board[sizeE] == _endBit)
            {
                for (you = sizeE - 1, own = 1; own <= sizeE; own++, you--)
                {
                    bit = 1UL;
                    for (ptr = _topBit; ptr != board[you] && board[own] >= bit; ptr >>= 1)
                        bit <<= 1;
                    if (board[own] > bit) return;
                    if (board[own] < bit) break;
                }
                if (own > sizeE)
                {
                    Count4++;
                    return;
                }
            }

            // 270-degree rotation.
            if (board[_bound1] == _topBit)
            {
                for (ptr = _topBit >> 1, own = 1; own <= sizeE; own++, ptr >>= 1)
                {
                    bit = 1UL;
                    for (you = 0; board[you] != ptr && board[own] >= bit; you++)
                        bit <<= 1;
                    if (board[own] > bit) return;
                    if (board[own] < bit) break;
                }
            }

            Count8++;
        }
    }
}
