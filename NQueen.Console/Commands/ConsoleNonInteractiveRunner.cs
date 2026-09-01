namespace NQueen.ConsoleApp.Commands;

public static class ConsoleNonInteractiveRunner
{
    public static bool HasSolverArgs(string[] args) =>
        args.Any(a => a.StartsWith("--mode", StringComparison.OrdinalIgnoreCase)
                   || a.StartsWith("--size", StringComparison.OrdinalIgnoreCase)
                   || a.Equals("--count-only", StringComparison.OrdinalIgnoreCase)
                   || a.Equals("--materialize", StringComparison.OrdinalIgnoreCase)
                   || a.Equals("--halfboard", StringComparison.OrdinalIgnoreCase)
                   || a.Equals("--help", StringComparison.OrdinalIgnoreCase)
                   || a.Equals("-h", StringComparison.OrdinalIgnoreCase));

    public static void Run(string[] args) =>
        Run(args, System.Console.Out);

    public static void Run(string[] args, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var options = ConsoleRunOptions.Parse(args);
        if (options.ShowHelp)
        {
            PrintHelp(output);
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

        output.WriteLine("NQueen Solver (non-interactive)");
        output.WriteLine($"  Mode            : {options.Mode}");
        output.WriteLine($"  Board Size      : {options.BoardSize}");
        output.WriteLine($"  Half-Board Flag : {(solver.EnableHalfBoardRestriction ? "ON" : "OFF")}");
        output.WriteLine($"  Count-Only      : {options.CountOnly}");
        output.WriteLine($"  Solutions Count : {results.SolutionsCount:N0}");
        output.WriteLine($"  Elapsed (sec)   : {results.ElapsedTimeInSec}");
        if (!options.CountOnly && results.Solutions.Count > 0)
        {
            output.WriteLine($"  Displayed ({results.Solutions.Count}):");
            foreach (var sol in results.Solutions)
                output.WriteLine($"    {sol.Name}: {sol.Details}");
        }
    }

    private static void PrintHelp(TextWriter output)
    {
        output.WriteLine("Usage: dotnet run --project NQueen.Console -- [options]\n");
        output.WriteLine("Options:");
        output.WriteLine("  --mode <all|unique|single>    Solution mode (default: all)");
        output.WriteLine("  --size <N>                     Board size (default: 8)");
        output.WriteLine("  --count-only                   Count solutions only (no materialization)");
        output.WriteLine("  --materialize                  Materialize sample solutions (default if --count-only omitted)");
        output.WriteLine("  --halfboard                    Legacy flag; All + Hide + N>=15 is automatic");
        output.WriteLine("  --help                         Show this help");
        output.WriteLine();
        output.WriteLine("Examples:");
        output.WriteLine("  Count All solutions N=15: dotnet run --project NQueen.Console -- --mode all --size 15 --count-only");
        output.WriteLine("  Materialize 5 sample Unique solutions N=12: dotnet run --project NQueen.Console -- --mode unique --size 12");
    }
}
