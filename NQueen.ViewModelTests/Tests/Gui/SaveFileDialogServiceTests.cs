using NQueen.GUI.Infrastructure;

namespace NQueen.ViewModelTests.Tests.Gui;

[Trait("Category", "Gui")]
public class SaveFileDialogServiceTests
{
    [Fact]
    public void WhenDialogAcceptedThenFileNameIsReturned()
    {
        var service = new SaveFileDialogService(() => "chosen.txt");

        service.ShowSaveFileDialog().ShouldBe("chosen.txt");
    }

    [Fact]
    public void WhenDialogCancelledThenNullIsReturned()
    {
        var service = new SaveFileDialogService(() => null);

        service.ShowSaveFileDialog().ShouldBeNull();
    }

    [Fact]
    public void WhenPathGivenThenContentIsWritten()
    {
        var path = Path.Combine(Path.GetTempPath(), $"nqueen-{Guid.NewGuid():N}.txt");

        new SaveFileDialogService().SaveContent(path, "board");

        File.ReadAllText(path).ShouldBe("board");
    }

    [Fact]
    public void WhenPathEmptyThenNothingIsWritten() => Should.NotThrow(() => new SaveFileDialogService().SaveContent(string.Empty, "board"));
}
