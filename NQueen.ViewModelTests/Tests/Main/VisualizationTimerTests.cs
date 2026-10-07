namespace NQueen.ViewModelTests.Tests.Main;

// Runs Visualize simulations on the WpfTestHost dispatcher so the DispatcherTimer actually ticks
// and the channel-drain / render path in MainViewModel.Events.cs is exercised end to end.
[Collection("Serial Test Collection")]
[Trait("Category", "Gui")]
public class VisualizationTimerTests
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(30);

    [Theory]
    [InlineData(6, SolutionMode.Single, 0)]
    [InlineData(6, SolutionMode.Single, 5)]
    [InlineData(6, SolutionMode.Unique, 0)]
    [InlineData(6, SolutionMode.All, 5)]
    [InlineData(9, SolutionMode.All, 0)]
    [InlineData(9, SolutionMode.Unique, 0)]
    public async Task WhenVisualizingOnDispatcherThenQueensAreRenderedDuringRun(int boardSize, SolutionMode mode, int delayMs)
    {
        var (vm, completed) = StartOnDispatcher(boardSize, mode, delayMs);

        int maxQueensSeen = await PollMaxQueensUntilAsync(vm, completed.Task);

        maxQueensSeen.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task WhenVisualizationCompletesThenFinalBoardShowsAllQueens()
    {
        const int boardSize = 6;
        var (vm, completed) = StartOnDispatcher(boardSize, SolutionMode.Single, 5);

        await completed.Task.WaitAsync(s_timeout);

        int queens = WpfTestHost.Run(() => CountQueens(vm));
        queens.ShouldBe(boardSize);
    }

    [Fact]
    public async Task WhenDelayChangedDuringVisualizationThenDelayIsClampedToDomainMinimum()
    {
        var (vm, completed) = StartOnDispatcher(6, SolutionMode.All, 5);

        int applied = WpfTestHost.Run(() =>
        {
            vm.DelayInMilliseconds = 1;
            return vm.DelayInMilliseconds;
        });
        await completed.Task.WaitAsync(s_timeout);

        applied.ShouldBe(SimulationSettings.MinDelayInMilliseconds);
    }

    private static (MainViewModel Vm, TaskCompletionSource<bool> Completed) StartOnDispatcher(
        int boardSize, SolutionMode mode, int delayMs)
    {
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = WpfTestHost.Run(() =>
        {
            var created = TestHelpers.CreateMainViewModel(
                boardSize: boardSize,
                solutionMode: mode,
                displayMode: DisplayMode.Visualize,
                suppressUserDialogs: true);
            created.DelayInMilliseconds = delayMs;
            created.SimulationCompleted += (_, _) => completed.TrySetResult(true);
            created.SimulateCommand.Execute(null);
            return created;
        });
        return (vm, completed);
    }

    private static async Task<int> PollMaxQueensUntilAsync(MainViewModel vm, Task completion)
    {
        int max = 0;
        var deadline = DateTime.UtcNow + s_timeout;
        while (!completion.IsCompleted)
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("Visualization did not complete in time.");
            max = Math.Max(max, WpfTestHost.Run(() => CountQueens(vm)));
            await Task.Delay(2);
        }
        return Math.Max(max, WpfTestHost.Run(() => CountQueens(vm)));
    }

    private static int CountQueens(MainViewModel vm) =>
        vm.ChessboardVm.Squares.Count(sq => !string.IsNullOrEmpty(sq.ImagePath));
}
