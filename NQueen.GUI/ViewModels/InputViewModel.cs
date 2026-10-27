namespace NQueen.GUI.ViewModels;

public sealed class InputViewModel(SolutionMode solutionMode)
{
    public FluentValidation.Results.ValidationResult ValidateBoardSize(string? boardSizeText)
    {
        boardSizeText ??= string.Empty;

        return _boardSizeValidator.Validate(boardSizeText);
    }

    private readonly BoardSizeValidator _boardSizeValidator = new(solutionMode);
}
