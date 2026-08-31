namespace NQueen.Benchmarking;

/// <summary>
/// Compares the comparable Console/GUI Hide-mode invocation shapes while keeping the benchmark
/// inside one BenchmarkDotNet process. This isolates solver configuration/invocation differences
/// from GUI, console, debugger, process startup, and manual-run noise.
/// </summary>
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[MemoryDiagnoser]
[ShortRunJob]
[WarmupCount(2)]
[IterationCount(5)]
public class FrontEndInvocationPathBenchmark
{
    [Params(12, 13)]
    public int BoardSize { get; set; }

    [Params(
        InvocationScenario.SingleMaterialize,
        InvocationScenario.UniqueMaterialize,
        InvocationScenario.UniqueCountOnly,
        InvocationScenario.AllMaterialize,
        InvocationScenario.AllCountOnly)]
    public InvocationScenario Scenario { get; set; }

    private readonly ISolutionFormatter _formatter = new NoopFormatter();

    [Benchmark(Baseline = true, Description = "Console-style direct Solve")]
    public ulong ConsoleStyleDirectSolve()
    {
        var mode = GetMode();
        var countOnly = IsCountOnly();
        using var solver = new BitmaskSolver(
            BoardSize,
            mode,
            DisplayMode.Hide,
            _formatter,
            maxSolutionsInOutput: countOnly ? 0 : SimulationSettings.MaxDisplayedCount)
        {
            EnableEvents = false,
        };

        BitmaskSolverRunConfigurator.Configure(
            solver,
            BoardSize,
            mode,
            DisplayMode.Hide,
            countOnly && mode == SolutionMode.All ? ResultStorageMode.CountOnly : ResultStorageMode.Materialize,
            countOnly && mode == SolutionMode.Unique ? ResultStorageMode.CountOnly : ResultStorageMode.Materialize);

        return solver.Solve().SolutionsCount;
    }

    [Benchmark(Description = "GUI-style backend async")]
    public ulong GuiStyleBackendAsync()
    {
        var mode = GetMode();
        var storageMode = GetStorageMode();
        using var solver = new BitmaskSolver(_formatter, enableCap: true)
        {
            EnableEvents = true,
        };

        BitmaskSolverRunConfigurator.Configure(
            solver,
            BoardSize,
            mode,
            DisplayMode.Hide,
            mode is SolutionMode.All or SolutionMode.Single ? storageMode : ResultStorageMode.Materialize,
            mode == SolutionMode.Unique ? storageMode : ResultStorageMode.Materialize);

        var context = new SimulationContext(
            BoardSize,
            mode,
            DisplayMode.Hide,
            OnProgress: new Progress<ProgressInfo>(_ => { }),
            Cancellation: CancellationToken.None,
            OnSolutionFound: new Progress<SolutionFoundInfo>(_ => { }),
            OnQueenPlaced: null,
            PauseGate: null);

        return solver.GetSimResultsAsync(context).GetAwaiter().GetResult().SolutionsCount;
    }

    private SolutionMode GetMode() => Scenario switch
    {
        InvocationScenario.SingleMaterialize => SolutionMode.Single,
        InvocationScenario.UniqueMaterialize or InvocationScenario.UniqueCountOnly => SolutionMode.Unique,
        InvocationScenario.AllMaterialize or InvocationScenario.AllCountOnly => SolutionMode.All,
        _ => throw new ArgumentOutOfRangeException(nameof(Scenario))
    };

    private ResultStorageMode GetStorageMode() => IsCountOnly()
        ? ResultStorageMode.CountOnly
        : ResultStorageMode.Materialize;

    private bool IsCountOnly() => Scenario is InvocationScenario.UniqueCountOnly or InvocationScenario.AllCountOnly;

    public enum InvocationScenario
    {
        SingleMaterialize,
        UniqueMaterialize,
        UniqueCountOnly,
        AllMaterialize,
        AllCountOnly
    }
}
