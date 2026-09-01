using NQueen.ConsoleApp.Commands;

namespace NQueen.UnitTests.Tests.Console;

[Trait("Category", "Console")]
public class ConsoleRunOptionsTests
{
    [Fact]
    public void Parse_UsesDefaults()
    {
        var options = ConsoleRunOptions.Parse([]);

        options.Mode.ShouldBe(SolutionMode.All);
        options.BoardSize.ShouldBe(8);
        options.CountOnly.ShouldBeFalse();
        options.DisplayedCap.ShouldBe(SimulationSettings.MaxDisplayedCount);
        options.ShowHelp.ShouldBeFalse();
    }

    [Fact]
    public void Parse_ReadsModeSizeAndCountOnly()
    {
        var options = ConsoleRunOptions.Parse(["--mode", "unique", "--size", "19", "--count-only"]);

        options.Mode.ShouldBe(SolutionMode.Unique);
        options.BoardSize.ShouldBe(19);
        options.CountOnly.ShouldBeTrue();
        options.DisplayedCap.ShouldBe(0);
    }

    [Fact]
    public void Parse_MaterializeOverridesCountOnly()
    {
        var options = ConsoleRunOptions.Parse(["--count-only", "--materialize"]);

        options.CountOnly.ShouldBeFalse();
        options.DisplayedCap.ShouldBe(SimulationSettings.MaxDisplayedCount);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    public void Parse_DetectsHelp(string flag)
    {
        var options = ConsoleRunOptions.Parse([flag]);

        options.ShowHelp.ShouldBeTrue();
    }

    [Fact]
    public void Parse_AcceptsLegacyHalfBoardAsNoOp()
    {
        var options = ConsoleRunOptions.Parse(["--mode", "unique", "--halfboard"]);

        options.Mode.ShouldBe(SolutionMode.Unique);
        options.CountOnly.ShouldBeFalse();
        options.DisplayedCap.ShouldBe(SimulationSettings.MaxDisplayedCount);
    }
}
