namespace NQueen.UnitTests.Tests.Solver;

[Trait("Category", "Solver")]
[Trait("Mode", "Config")]
public class BitmaskSolverRunConfiguratorTests
{
    private static BitmaskSolver MakeSolver(int boardSize = 8, SolutionMode mode = SolutionMode.All, DisplayMode displayMode = DisplayMode.Hide) =>
        new(boardSize, mode, displayMode, new SolutionFormatter()) { EnableEvents = false };

    [Theory]
    [InlineData(13, SolutionMode.Single, DisplayMode.Hide, false)]
    [InlineData(14, SolutionMode.Single, DisplayMode.Hide, true)]
    [InlineData(8, SolutionMode.All, DisplayMode.Hide, false)]
    [InlineData(9, SolutionMode.All, DisplayMode.Hide, true)]
    [InlineData(19, SolutionMode.Unique, DisplayMode.Visualize, false)]
    public void ComputeUseParallel_AppliesSharedThresholds(int boardSize, SolutionMode mode, DisplayMode displayMode, bool expected) =>
        BitmaskSolverRunConfigurator.ComputeUseParallel(boardSize, mode, displayMode).ShouldBe(expected);

    [Theory]
    [InlineData(8, true, 1)]
    [InlineData(12, true, 2)]
    [InlineData(16, true, 3)]
    [InlineData(16, false, 1)]
    public void ComputeParallelRootSplitDepth_AppliesBoardSizeBands(int boardSize, bool useParallel, int expected) =>
        BitmaskSolverRunConfigurator.ComputeParallelRootSplitDepth(boardSize, useParallel).ShouldBe(expected);

    [Theory]
    [InlineData(14, SolutionMode.All, DisplayMode.Hide, false)]
    [InlineData(15, SolutionMode.All, DisplayMode.Hide, true)]
    [InlineData(19, SolutionMode.All, DisplayMode.Visualize, false)]
    [InlineData(19, SolutionMode.Unique, DisplayMode.Hide, false)]
    public void ComputeHalfBoardRestriction_AppliesOnlyAllHideLargeBoards(int boardSize, SolutionMode mode, DisplayMode displayMode, bool expected) =>
        BitmaskSolverRunConfigurator.ComputeHalfBoardRestriction(boardSize, mode, displayMode).ShouldBe(expected);

    [Fact]
    public void Configure_AllCountOnlyHide_SetsSharedRuntimeFlags()
    {
        using var solver = MakeSolver(boardSize: 15, mode: SolutionMode.All);

        BitmaskSolverRunConfigurator.Configure(
            solver,
            boardSize: 15,
            solutionMode: SolutionMode.All,
            displayMode: DisplayMode.Hide,
            allStorageMode: ResultStorageMode.CountOnly,
            uniqueStorageMode: ResultStorageMode.Materialize);

        solver.UseParallel.ShouldBeTrue();
        solver.ParallelRootSplitDepth.ShouldBe(2);
        solver.AllStorageMode.ShouldBe(ResultStorageMode.CountOnly);
        solver.EnableHalfBoardRestriction.ShouldBeTrue();
        solver.EnablePrefixMinimalityPruning.ShouldBeTrue();
        solver.EnablePartialReflectionPruning.ShouldBeTrue();
        solver.UseAdaptiveDepth.ShouldBeTrue();
        solver.UseCountOnlyAllMode.ShouldBeTrue();
        solver.UseCountOnlyUniqueMode.ShouldBeFalse();
    }

    [Fact]
    public void Configure_UniqueCountOnlyHide_SetsUniqueCountOnlyWithoutHalfBoardFlag()
    {
        using var solver = MakeSolver(boardSize: 16, mode: SolutionMode.Unique);

        BitmaskSolverRunConfigurator.Configure(
            solver,
            boardSize: 16,
            solutionMode: SolutionMode.Unique,
            displayMode: DisplayMode.Hide,
            allStorageMode: ResultStorageMode.Materialize,
            uniqueStorageMode: ResultStorageMode.CountOnly);

        solver.UniqueStorageMode.ShouldBe(ResultStorageMode.CountOnly);
        solver.EnableHalfBoardRestriction.ShouldBeFalse();
        solver.UseCountOnlyAllMode.ShouldBeFalse();
        solver.UseCountOnlyUniqueMode.ShouldBeTrue();
    }

    [Fact]
    public void Configure_Visualize_DisablesParallelAndCountOnlyFlags()
    {
        using var solver = MakeSolver(boardSize: 16, mode: SolutionMode.All, displayMode: DisplayMode.Visualize);

        BitmaskSolverRunConfigurator.Configure(
            solver,
            boardSize: 16,
            solutionMode: SolutionMode.All,
            displayMode: DisplayMode.Visualize,
            allStorageMode: ResultStorageMode.CountOnly,
            uniqueStorageMode: ResultStorageMode.CountOnly);

        solver.UseParallel.ShouldBeFalse();
        solver.ParallelRootSplitDepth.ShouldBe(1);
        solver.EnableHalfBoardRestriction.ShouldBeFalse();
        solver.UseCountOnlyAllMode.ShouldBeFalse();
        solver.UseCountOnlyUniqueMode.ShouldBeFalse();
    }
}
