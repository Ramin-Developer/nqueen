namespace NQueen.Benchmarking;

internal class Program
{
    private const string BenchmarkModeEnvironmentVariable = "BENCHMARK_MODE";
    private const string BenchmarkProfileEnvironmentVariable = "BENCHMARK_PROFILE";
    private const string DefaultProfile = "canonical";

    private static readonly IReadOnlyDictionary<string, Type[]> s_benchmarkProfiles = new Dictionary<string, Type[]>(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultProfile] =
        [
            typeof(UniqueFastHalfBoardEvenOddBenchmark),
            typeof(AllCountOnlyParallelScalingBenchmark),
            typeof(AllCountOnlyRecursiveVsIterativeBenchmark),
            typeof(FrontEndInvocationPathBenchmark)
        ],
        ["unique"] = [typeof(UniqueFastHalfBoardEvenOddBenchmark)],
        ["all"] = [typeof(AllCountOnlyParallelScalingBenchmark), typeof(AllCountOnlyRecursiveVsIterativeBenchmark)],
        ["frontend"] = [typeof(FrontEndInvocationPathBenchmark)],
        ["parity"] = [typeof(ModeParityBenchmark)],
        ["console"] = [typeof(ConsolePruningImpactAllBenchmark), typeof(ConsolePruningImpactUniqueBenchmark)]
    };

    private static void Main(string[] args)
    {
        Console.WriteLine("Running NQueen benchmarks (Release)...");
        var benchMode = Environment.GetEnvironmentVariable(BenchmarkModeEnvironmentVariable);
        if ((benchMode == "1") || args.Any(a => a.StartsWith("--", StringComparison.Ordinal)))
        {
            // Run all benchmarks in this assembly when invoked by BenchmarkDotNet infrastructure.
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            Console.WriteLine("Done. See BenchmarkDotNet.Artifacts for detailed reports.");
            return;
        }

        var profile = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
            ?? Environment.GetEnvironmentVariable(BenchmarkProfileEnvironmentVariable)
            ?? DefaultProfile;

        RunProfile(profile);
        Console.WriteLine("Done. See BenchmarkDotNet.Artifacts for detailed reports.");
    }

    private static void RunProfile(string profile)
    {
        if (!s_benchmarkProfiles.TryGetValue(profile, out var benchmarkTypes))
        {
            var availableProfiles = string.Join(", ", s_benchmarkProfiles.Keys.Order(StringComparer.OrdinalIgnoreCase));
            throw new ArgumentException($"Unknown benchmark profile '{profile}'. Available profiles: {availableProfiles}.");
        }

        Console.WriteLine($"Profile: {profile}");
        foreach (var benchmarkType in benchmarkTypes)
            Console.WriteLine($" - {benchmarkType.Name}");

        BenchmarkSwitcher.FromTypes(benchmarkTypes).Run(["--filter", "*"]);
    }
}
