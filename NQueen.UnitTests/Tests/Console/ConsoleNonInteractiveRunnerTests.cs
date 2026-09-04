using NQueen.ConsoleApp.Commands;

namespace NQueen.UnitTests.Tests.Console;

[Trait("Category", "Console")]
public class ConsoleNonInteractiveRunnerTests
{
    [Theory]
    [InlineData("--mode")]
    [InlineData("--size")]
    [InlineData("--count-only")]
    [InlineData("--materialize")]
    [InlineData("--halfboard")]
    [InlineData("--help")]
    [InlineData("-h")]
    public void HasSolverArgs_DetectsKnownFlags(string flag) =>
        ConsoleNonInteractiveRunner.HasSolverArgs([flag]).ShouldBeTrue();

    [Fact]
    public void HasSolverArgs_ReturnsFalseForUnknownFlags() =>
        ConsoleNonInteractiveRunner.HasSolverArgs(["--unknown"]).ShouldBeFalse();

    [Theory]
    [InlineData("--model")]
    [InlineData("--sizeLimit")]
    public void HasSolverArgs_DoesNotTreatPrefixMatchesAsKnownFlags(string flag) =>
        ConsoleNonInteractiveRunner.HasSolverArgs([flag]).ShouldBeFalse();

    [Fact]
    public void Run_Help_WritesUsageWithoutSolving()
    {
        using var writer = new StringWriter();

        ConsoleNonInteractiveRunner.Run(["--help"], writer);

        var output = writer.ToString();
        output.ShouldContain("Usage: dotnet run --project NQueen.Console -- [options]");
        output.ShouldContain("--halfboard                    Legacy flag; All + Hide + N>=15 is automatic");
        output.ShouldNotContain("Solutions Count");
    }

    [Fact]
    public void Run_SmallSingle_WritesExpectedSummary()
    {
        using var writer = new StringWriter();

        ConsoleNonInteractiveRunner.Run(["--mode", "single", "--size", "4"], writer);

        var output = writer.ToString();
        output.ShouldContain("NQueen Solver (non-interactive)");
        output.ShouldContain("Mode            : Single");
        output.ShouldContain("Board Size      : 4");
        output.ShouldContain("Solutions Count : 1");
    }

    [Fact]
    public void Run_UniqueAboveKnownCountRange_WritesErrorWithoutSolving()
    {
        using var writer = new StringWriter();

        ConsoleNonInteractiveRunner.Run(["--mode", "unique", "--size", (BoardSettings.MaxSizeForUnique + 1).ToString(), "--count-only"], writer);

        var output = writer.ToString();
        output.ShouldContain($"Invalid board size. Enter a value from {BoardSettings.MinSize} to {BoardSettings.MaxSizeForUnique}.");
        output.ShouldNotContain("Solutions Count");
    }
}
