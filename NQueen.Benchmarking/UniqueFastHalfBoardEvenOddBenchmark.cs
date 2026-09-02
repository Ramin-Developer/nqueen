namespace NQueen.Benchmarking;

using Microsoft.VSDiagnostics;

/// <summary>
/// Focused isolation of the Unique count-only fast half-board path at N=16/N=18 (even) and
/// N=17 (odd). Mirrors <see cref="UniqueFastHalfBoardBenchmark"/>'s solver configuration
/// but pins board sizes to 16, 17, and 18 so the run exercises <c>CountUniqueFastHalfBoard</c>
/// -> <c>CountCanonicalDFS</c> (the Item 2 gating hot loop) without the N=15 case that
/// routes through a different path. Covering both parities exercises the even path and the
/// odd-center first-row handling (<c>IsOddCenterFirstRow</c>). Uses a full job (3 warmups,
/// 15 measured iterations) so this can serve as a stable, low-variance canonical baseline
/// for future kernel hot-loop work — the earlier short job produced means that were
/// reproducible but with wide confidence intervals.
/// </summary>
[SimpleJob(warmupCount: 3, iterationCount: 15)]
[CPUUsageDiagnoser]
public class UniqueFastHalfBoardEvenOddBenchmark
{
    [Params(16, 17, 18)]
    public int BoardSize { get; set; }

    [Benchmark(Description = "Unique Count-Only Fast Half-Board (N=16,17,18)")]
    public ulong Unique_CountOnly_HalfBoard()
    {
        using var solver = new BitmaskSolver(BoardSize, SolutionMode.Unique, DisplayMode.Hide, new NoopFormatter())
        {
            EnableEvents = false,
            UseParallel = true,
            UseCountOnlyUniqueMode = true,
            EnablePrefixMinimalityPruning = true,
            EnablePartialReflectionPruning = true,
            EnableHalfBoardRestriction = BoardSize >= 15
        };
        return solver.Solve().SolutionsCount;
    }
}
