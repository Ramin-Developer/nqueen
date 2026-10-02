using NQueen.ConsoleApp;
using NQueen.ConsoleApp.Services;

namespace NQueen.UnitTests.Tests.Console;

[CollectionDefinition(nameof(ConsoleIoCollection), DisableParallelization = true)]
public class ConsoleIoCollection;

[Trait("Category", "Console")]
[Collection(nameof(ConsoleIoCollection))]
public class InteractiveMenuTests
{
    private static string RunMenu(string script, bool registerFormatter = true)
    {
        var services = new ServiceCollection();
        if (registerFormatter)
            services.AddNQueenServices(enableCap: true);
        services.AddSingleton<App>();
        using var provider = services.BuildServiceProvider();

        return CaptureConsole(script, () => provider.GetRequiredService<App>().Run().GetAwaiter().GetResult());
    }

    private static string CaptureConsole(string script, Action action)
    {
        var originalIn = System.Console.In;
        var originalOut = System.Console.Out;
        using var reader = new StringReader(script);
        using var writer = new StringWriter();
        try
        {
            System.Console.SetIn(reader);
            System.Console.SetOut(writer);
            action();
        }
        finally
        {
            System.Console.SetIn(originalIn);
            System.Console.SetOut(originalOut);
        }
        return writer.ToString();
    }

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines) + Environment.NewLine;

    [Fact]
    public void SingleMode_PrintsSummaryWithExampleSolution()
    {
        var output = RunMenu(Lines("1", "8", "e"));

        output.ShouldContain("Mode            : Single");
        output.ShouldContain("Solution 1:");
    }

    [Theory]
    [InlineData("2", "y", "Total Solutions : 12")]
    [InlineData("2", "n", "Total Solutions : 12")]
    [InlineData("3", "y", "Total Solutions : 92")]
    [InlineData("3", "n", "Total Solutions : 92")]
    public void UniqueAndAllModes_ReportCanonicalCountForN8(string modeChoice, string countOnly, string expected)
    {
        var output = RunMenu(Lines(modeChoice, "8", countOnly, "e"));

        output.ShouldContain(expected);
    }

    [Fact]
    public void AllMaterialize_ShowsExampleSolutions()
    {
        var output = RunMenu(Lines("3", "6", "n", "e"));

        output.ShouldContain("Showing up to");
    }

    [Fact]
    public void FollowUpBoardSize_RunsSecondSimulation()
    {
        var output = RunMenu(Lines("2", "6", "y", "8", "e"));

        output.ShouldContain("Total Solutions : 1");
        output.ShouldContain("Total Solutions : 12");
    }

    [Fact]
    public void FollowUpInvalidBoardSize_PrintsValidationMessage()
    {
        var output = RunMenu(Lines("1", "4", "abc", "e"));

        output.ShouldContain("Invalid board size.");
    }

    [Fact]
    public void BackCommand_ReturnsToBoardSizePrompt()
    {
        var output = RunMenu(Lines("1", "4", "b", "0", "e"));

        output.ShouldContain("Enter board size");
    }

    [Theory]
    [InlineData("9")]
    [InlineData("x")]
    public void InvalidModeChoice_PrintsInvalidChoiceAndExits(string choice)
    {
        var output = RunMenu(Lines(choice));

        output.ShouldContain("Invalid choice. Try again.");
    }

    [Theory]
    [InlineData("q")]
    [InlineData("exit")]
    [InlineData("0")]
    public void QuitInputAtModeMenu_ExitsWithoutSimulating(string input)
    {
        var output = RunMenu(Lines(input));

        output.ShouldNotContain("Summary:");
    }

    [Fact]
    public void TwoBlankInputsAtModeMenu_Exit()
    {
        var output = RunMenu(Lines("", ""));

        output.ShouldNotContain("Summary:");
    }

    [Fact]
    public void InvalidBoardSize_PrintsValidationMessage()
    {
        var output = RunMenu(Lines("1", "0x", "q"));

        output.ShouldContain("Invalid board size.");
    }

    [Fact]
    public void ZeroAtBoardSizeMenu_ReturnsToModeMenu()
    {
        var output = RunMenu(Lines("1", "0", "q"));

        output.ShouldNotContain("Summary:");
    }

    [Fact]
    public void MissingFormatter_PrintsErrorAndExits()
    {
        var output = RunMenu(Lines("1", "4"), registerFormatter: false);

        output.ShouldContain("ISolutionFormatter service not found");
    }

    [Fact]
    public void ProgramMain_WithSolverArgs_RunsNonInteractivePath()
    {
        var output = CaptureConsole(string.Empty, () => Program.Main(["--mode", "unique", "--size", "8"]).GetAwaiter().GetResult());

        output.ShouldContain("12");
    }

    [Fact]
    public void ProgramMain_WithoutArgs_RunsInteractiveMenu()
    {
        var output = CaptureConsole(Lines("q"), () => Program.Main([]).GetAwaiter().GetResult());

        output.ShouldContain("Select Solution Mode");
    }
}
