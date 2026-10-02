namespace NQueen.Benchmarking;

/// <summary>
/// Evidence baseline for the solver-mode parity pass: measures Single, All, and Unique in both
/// storage modes through the shared Kernel configurator so the non-Unique-CountOnly paths can be
/// compared against the already-optimized Unique CountOnly path.
/// </summary>
[Orderer(SummaryOrderPolicy.Declared)]
[MemoryDiagnoser]
[ShortRunJob]
[WarmupCount(2)]
[IterationCount(5)]
public class ModeParityBenchmark
{
    [Params(14, 15, 16)]
    public int BoardSize { get; set; }

    [Params(
        FrontEndInvocationPathBenchmark.InvocationScenario.SingleMaterialize,
        FrontEndInvocationPathBenchmark.InvocationScenario.AllCountOnly,
        FrontEndInvocationPathBenchmark.InvocationScenario.AllMaterialize,
        FrontEndInvocationPathBenchmark.InvocationScenario.UniqueCountOnly,
        FrontEndInvocationPathBenchmark.InvocationScenario.UniqueMaterialize)]
    public FrontEndInvocationPathBenchmark.InvocationScenario Scenario { get; set; }

    private readonly ISolutionFormatter _formatter = new NoopFormatter();

    [Benchmark]
    public ulong Solve()
    {
        var (mode, countOnly) = Scenario switch
        {
            FrontEndInvocationPathBenchmark.InvocationScenario.SingleMaterialize => (SolutionMode.Single, false),
            FrontEndInvocationPathBenchmark.InvocationScenario.AllCountOnly => (SolutionMode.All, true),
            FrontEndInvocationPathBenchmark.InvocationScenario.AllMaterialize => (SolutionMode.All, false),
            FrontEndInvocationPathBenchmark.InvocationScenario.UniqueCountOnly => (SolutionMode.Unique, true),
            _ => (SolutionMode.Unique, false),
        };

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
}
