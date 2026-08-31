namespace NQueen.Kernel.Solvers;

public static class BitmaskSolverRunConfigurator
{
    public static void Configure(
        BitmaskSolver solver,
        int boardSize,
        SolutionMode solutionMode,
        DisplayMode displayMode,
        ResultStorageMode allStorageMode,
        ResultStorageMode uniqueStorageMode)
    {
        ArgumentNullException.ThrowIfNull(solver);

        var isVisualized = displayMode == DisplayMode.Visualize;
        var useParallel = ComputeUseParallel(boardSize, solutionMode, displayMode);

        solver.UseParallel = useParallel;
        solver.ParallelRootSplitDepth = ComputeParallelRootSplitDepth(boardSize, useParallel);
        solver.AllStorageMode = solutionMode is SolutionMode.All or SolutionMode.Single
            ? allStorageMode
            : solver.AllStorageMode;
        solver.UniqueStorageMode = solutionMode == SolutionMode.Unique
            ? uniqueStorageMode
            : solver.UniqueStorageMode;
        solver.EnableHalfBoardRestriction = ComputeHalfBoardRestriction(boardSize, solutionMode, displayMode);
        solver.EnablePrefixMinimalityPruning = true;
        solver.EnablePartialReflectionPruning = true;
        solver.UseAdaptiveDepth = boardSize >= 14;
        solver.UseCountOnlyAllMode = !isVisualized && solutionMode == SolutionMode.All && solver.AllStorageMode == ResultStorageMode.CountOnly;
        solver.UseCountOnlyUniqueMode = !isVisualized && solutionMode == SolutionMode.Unique && solver.UniqueStorageMode == ResultStorageMode.CountOnly;
    }

    public static bool ComputeUseParallel(int boardSize, SolutionMode solutionMode, DisplayMode displayMode)
    {
        if (displayMode == DisplayMode.Visualize)
            return false;
        return solutionMode == SolutionMode.Single
            ? boardSize >= 14
            : boardSize >= 9;
    }

    public static int ComputeParallelRootSplitDepth(int boardSize, bool useParallel)
    {
        if (!useParallel) return 1;
        var depth = boardSize < 12 ? 1 : boardSize < 16 ? 2 : 3;
        return Math.Min(depth, Math.Max(1, boardSize));
    }

    public static bool ComputeHalfBoardRestriction(int boardSize, SolutionMode solutionMode, DisplayMode displayMode) =>
        solutionMode == SolutionMode.All && displayMode != DisplayMode.Visualize && boardSize >= 15;
}
