namespace NQueen.ViewModelTests.Tests.Main;

[Trait("Category", "ViewModel")]
public class MainViewModelPauseTests
{
    [Theory]
    [InlineData(6, SolutionMode.Single, DisplayMode.Visualize, true)]
    [InlineData(8, SolutionMode.Single, DisplayMode.Visualize, true)]
    [InlineData(9, SolutionMode.Single, DisplayMode.Visualize, false)]
    [InlineData(6, SolutionMode.Single, DisplayMode.Hide, false)]
    [InlineData(6, SolutionMode.Unique, DisplayMode.Visualize, false)]
    [InlineData(6, SolutionMode.All, DisplayMode.Visualize, false)]
    public void PauseButtonVisibility_ShouldBeVisible_OnlyForVisualizedSingleUpToEight(
        int boardSize, SolutionMode solutionMode, DisplayMode displayMode, bool expectedVisible)
    {
        var vm = TestHelpers.CreateMainViewModel(boardSize, solutionMode, displayMode);

        var expected = expectedVisible ? Visibility.Visible : Visibility.Collapsed;
        vm.PauseButtonVisibility.ShouldBe(expected);
    }

    [Fact]
    public void PauseButtonLabel_ShouldDefaultToStop()
    {
        var vm = TestHelpers.CreateMainViewModel(6, SolutionMode.Single, DisplayMode.Visualize);

        vm.IsPaused.ShouldBeFalse();
        vm.PauseButtonLabel.ShouldBe("Stop");
    }

    [Fact]
    public void PauseButtonLabel_ShouldFlipToResume_WhenPaused()
    {
        var vm = TestHelpers.CreateMainViewModel(6, SolutionMode.Single, DisplayMode.Visualize);

        vm.IsPaused = true;

        vm.PauseButtonLabel.ShouldBe("Resume");
    }

    [Fact]
    public void CanTogglePause_ShouldBeFalse_WhenNotSimulating()
    {
        var vm = TestHelpers.CreateMainViewModel(6, SolutionMode.Single, DisplayMode.Visualize);

        vm.TogglePauseCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public async Task TogglePause_ShouldFlipStateAndGate_DuringVisualizedSingleRun()
    {
        // Arrange: a mock solver that blocks inside GetSimResultsAsync until released, so the
        // run stays active while we exercise the pause gate created by the ViewModel.
        var mockFormatter = new Mock<ISolutionFormatter>().Object;
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        SimulationContext? captured = null;

        var mockSolver = new Mock<ISolver>();
        mockSolver
            .Setup(s => s.GetSimResultsAsync(It.IsAny<SimulationContext>()))
            .Returns<SimulationContext>(async ctx =>
            {
                captured = ctx;
                await release.Task;
                return new SimulationResults(
                    [new Solution([1, 3, 5, 0, 2, 4], mockFormatter, null)], 1.0);
            });

        var vm = TestHelpers.CreateMainViewModelWithMock(
            mockSolver.Object,
            new SimulationContext(6, SolutionMode.Single, DisplayMode.Visualize));

        // Act: start the run without awaiting; it parks on release.Task.
        var runTask = vm.SimulateCommand.ExecuteAsync(null);
        await TestHelpers.WaitForConditionAsync(
            () => vm.IsSimulating && captured?.PauseGate != null, TimeSpan.FromSeconds(5));

        var gate = captured!.PauseGate!;

        // Assert: toggle available, Stop pauses (gate reset), Resume runs (gate set).
        vm.TogglePauseCommand.CanExecute(null).ShouldBeTrue();

        vm.TogglePauseCommand.Execute(null);
        vm.IsPaused.ShouldBeTrue();
        vm.PauseButtonLabel.ShouldBe("Resume");
        gate.IsSet.ShouldBeFalse();

        vm.TogglePauseCommand.Execute(null);
        vm.IsPaused.ShouldBeFalse();
        vm.PauseButtonLabel.ShouldBe("Stop");
        gate.IsSet.ShouldBeTrue();

        // Cleanup: release the solver and let the run finish.
        release.TrySetResult(true);
        await runTask;
    }
}
