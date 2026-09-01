namespace NQueen.ConsoleApp.Commands;

public sealed record ConsoleRunOptions(
    SolutionMode Mode,
    int BoardSize,
    bool CountOnly,
    int DisplayedCap,
    bool ShowHelp)
{
    public static ConsoleRunOptions Parse(string[] args)
    {
        var mode = SolutionMode.All;
        int size = 8;
        bool countOnly = false;
        int displayedCap = SimulationSettings.MaxDisplayedCount;
        bool showHelp = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].Trim();
            switch (arg.ToLowerInvariant())
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--mode":
                    if (i + 1 < args.Length)
                    {
                        mode = ParseMode(args[++i], mode);
                    }
                    break;
                case "--size":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var n)) size = n;
                    break;
                case "--count-only":
                    countOnly = true;
                    displayedCap = 0;
                    break;
                case "--materialize":
                    countOnly = false;
                    displayedCap = SimulationSettings.MaxDisplayedCount;
                    break;
                case "--halfboard":
                    break;
            }
        }

        return new ConsoleRunOptions(mode, size, countOnly, displayedCap, showHelp);
    }

    private static SolutionMode ParseMode(string value, SolutionMode fallback) =>
        value.Trim().ToLowerInvariant() switch
        {
            "all" => SolutionMode.All,
            "unique" => SolutionMode.Unique,
            "single" => SolutionMode.Single,
            _ => fallback
        };
}
