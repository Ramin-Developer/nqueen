namespace NQueen.ViewModelTests.Tests.Main;

[Trait("Category", "Chessboard")]
public class ChessboardViewModelTests
{
    private static ChessboardViewModel CreateChessboard(double windowSize = 800)
    {
        var dispatcher = new Mock<IDispatcher>();
        return new ChessboardViewModel(dispatcher.Object)
        {
            WindowWidth = windowSize,
            WindowHeight = windowSize,
        };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(15)]
    public void CreateSquares_ShouldPopulateBoardSizeSquaredSquares(int boardSize)
    {
        // Arrange
        var chessboard = CreateChessboard();

        // Act
        chessboard.CreateSquares(boardSize);

        // Assert: the layout dimension and the populated squares must always agree,
        // otherwise the UniformGrid would lay squares out against the wrong dimension.
        chessboard.BoardDimension.ShouldBe(boardSize);
        chessboard.Squares.Count.ShouldBe(boardSize * boardSize);
        chessboard.Squares.Count.ShouldBe(chessboard.BoardDimension * chessboard.BoardDimension);
    }

    [Fact]
    public void CreateSquares_ShouldKeepDimensionAndSquaresConsistent_AcrossResizes()
    {
        // Arrange
        var chessboard = CreateChessboard();

        // Act & Assert: rebuilding for different sizes in sequence must always leave the
        // dimension and square count in agreement (guards the distortion regression).
        foreach (var size in new[] { 8, 4, 12, 1, 10 })
        {
            chessboard.CreateSquares(size);

            chessboard.BoardDimension.ShouldBe(size);
            chessboard.Squares.Count.ShouldBe(size * size);
        }
    }
}
