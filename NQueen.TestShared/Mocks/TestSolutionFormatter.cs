namespace NQueen.TestShared.Mocks;

public sealed class TestSolutionFormatter : ISolutionFormatter
{
    public string FormatSolutions(
        IReadOnlyList<Position> positions,
        IndexingType indexingType = IndexingType.OneBased,
        int noOfQueensPerLine = 40) =>
        // Return a predictable string for testing
        $"Formatted {positions.Count} positions";
}