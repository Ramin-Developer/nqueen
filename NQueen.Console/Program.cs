namespace NQueen.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Non-interactive fast path when flags supplied (skip menu)
        if (args.Length > 0 && HasSolverArgs(args))
        {
            RunNonInteractive(args);
            return;
        }

        using var serviceProvider = ConfigureServices();
        var app = serviceProvider.GetRequiredService<App>();
        await app.Run(args); // interactive menu
    }

    private static bool HasSolverArgs(string[] args)
    {
        // Treat presence of any recognized flag as non-interactive intent
        return args.Any(a => a.StartsWith("--mode", StringComparison.OrdinalIgnoreCase)
                          || a.StartsWith("--size", StringComparison.OrdinalIgnoreCase)
                          || a.Equals("--count-only", StringComparison.OrdinalIgnoreCase)
                          || a.Equals("--halfboard", StringComparison.OrdinalIgnoreCase)
                          || a.Equals("--help", StringComparison.OrdinalIgnoreCase));
    }

    private static void RunNonInteractive(string[] args)
    {
        var options = ConsoleRunOptions.Parse(args);
        if (options.ShowHelp)
        {
            PrintHelp();
            return;
        }

        var formatter = new SolutionFormatter();
        using var solver = new BitmaskSolver(options.BoardSize, options.Mode, DisplayMode.Hide, formatter, maxSolutionsInOutput: options.DisplayedCap)
        {
            EnableEvents = false,
        };
        BitmaskSolverRunConfigurator.Configure(
            solver,
            options.BoardSize,
            options.Mode,
            DisplayMode.Hide,
            options.CountOnly && options.Mode == SolutionMode.All ? ResultStorageMode.CountOnly : ResultStorageMode.Materialize,
            options.CountOnly && options.Mode == SolutionMode.Unique ? ResultStorageMode.CountOnly : ResultStorageMode.Materialize);
        var results = solver.Solve();

        Console.WriteLine("NQueen Solver (non-interactive)");
        Console.WriteLine($"  Mode            : {options.Mode}");
        Console.WriteLine($"  Board Size      : {options.BoardSize}");
        Console.WriteLine($"  Half-Board Flag : {(solver.EnableHalfBoardRestriction ? "ON" : "OFF")}");
        Console.WriteLine($"  Count-Only      : {options.CountOnly}");
        Console.WriteLine($"  Solutions Count : {results.SolutionsCount:N0}");
        Console.WriteLine($"  Elapsed (sec)   : {results.ElapsedTimeInSec}");
        if (!options.CountOnly && results.Solutions.Count > 0)
        {
            Console.WriteLine($"  Displayed ({results.Solutions.Count}):");
            foreach (var sol in results.Solutions)
                Console.WriteLine($"    {sol.Name}: {sol.Details}");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: dotnet run --project NQueen.Console -- [options]\n");
        Console.WriteLine("Options:");
        Console.WriteLine("  --mode <all|unique|single>    Solution mode (default: all)");
        Console.WriteLine("  --size <N>                     Board size (default: 8)");
        Console.WriteLine("  --count-only                   Count solutions only (no materialization)");
        Console.WriteLine("  --materialize                  Materialize sample solutions (default if --count-only omitted)");
        Console.WriteLine("  --halfboard                    Legacy flag; All + Hide + N>=15 is automatic");
        Console.WriteLine("  --help                         Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Count All solutions N=15: dotnet run --project NQueen.Console -- --mode all --size 15 --count-only");
        Console.WriteLine("  Materialize 5 sample Unique solutions N=12: dotnet run --project NQueen.Console -- --mode unique --size 12");
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core application registrations
        services.AddNQueenServices(enableCap: true);

        // Root app
        services.AddSingleton<App>();

        return services.BuildServiceProvider();
    }
}
