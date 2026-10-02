namespace NQueen.Kernel.Solvers;

public partial class BitmaskSolver
{
    // Wrapper used by HandleModeCommon to select parallel mode and split depth.
    private void EnumerateAllAdaptive(bool countOnly)
    {
        if (countOnly)
        {
            _solutionCount = CountAllFast(BoardSize);
            RaiseProgress(100.0);
            return;
        }

        // A full sequential enumeration visits every one of the ~39B solutions (N=20) with
        // no half-board symmetry reduction (e.g. 1343s for N=20). The two-phase approach:
        //   1. Early-exit DFS to collect the display sample (near-instant).
        //   2. BitboardNQueenSolver.CountSolutions with half-board symmetry (~19.5B nodes
        //      instead of 39B) — roughly 2x faster even when sequential, and much faster
        //      in parallel.
        // Remove the UseParallel guard so both parallel and sequential configurations
        // benefit; UseParallel is passed through to CountSolutions inside.
        // Applies to every N: the full sequential enumeration was ~20-60x slower than the
        // fast counter at N=12/13.
        CollectAllSamplesAndCountParallel();
    }

    // N >= 8: symmetry-class counter (All = 2*C2 + 4*C4 + 8*C8); smaller boards keep
    // the half-board bitboard counter.
    private ulong CountAllFast(int n) =>
        n >= SimulationSettings.UniqueCountOnlyParallelThresholdN
            ? CountAllSymmetryClass(n)
            : (ulong)BitboardNQueenSolver.CountSolutions(n, parallel: true);

    // Phase 1: collect up to cap solutions via an early-exit DFS (completes in milliseconds).
    // Phase 2: count all solutions with the parallel half-board bitboard counter.
    private void CollectAllSamplesAndCountParallel()
    {
        int N = BoardSize;
        // Always collect at least one sample regardless of
        // _capEnabled (uncapped test solvers still need solutions in the result).
        int cap = Math.Max(1, SimulationSettings.MaxDisplayedCount);
        CollectAllSampleSolutionsDFS(N, cap);

        _solutionCount = CountAllFast(N);
        RaiseProgress(100.0);
    }

    // Runs a minimal DFS that stops as soon as cap solutions are stored.
    private void CollectAllSampleSolutionsDFS(int N, int cap)
    {
        ulong mask = N == 64 ? ulong.MaxValue : (1UL << N) - 1UL;
        int[] rows = new int[N];
        Array.Fill(rows, -1);
        int materialized = 0;

        DFS(0, 0UL, 0UL, 0UL);

        void DFS(int col, ulong cols, ulong d1, ulong d2)
        {
            if (materialized >= cap || IsCancellationRequested) return;
            if (col == N)
            {
                if (N <= 25)
                {
                    // Store the ACTUAL board (raw packing); All-mode samples must stay distinct,
                    // whereas a canonical key would collapse different boards onto one placement.
                    var packed = SymmetryHelper.PackRows(rows);
                    _solutions.Add((packed, N));
                }
                else
                {
                    var copy = new int[N];
                    Array.Copy(rows, copy, N);
                    _largeBoardRawSolutions.Add(copy);
                }
                RaiseSolutionFound(rows, N);
                materialized++;
                if (materialized >= cap)
                    _eventsSuppressedAfterCap = true;
                return;
            }
            ulong avail = ~(cols | d1 | d2) & mask;
            while (avail != 0 && materialized < cap && !IsCancellationRequested)
            {
                ulong bit = avail & (ulong)-(long)avail;
                avail ^= bit;
                rows[col] = BitOperations.TrailingZeroCount(bit);
                DFS(col + 1, cols | bit, (d1 | bit) << 1, (d2 | bit) >> 1);
            }
            rows[col] = -1;
        }
    }
}
