namespace NQueen.GUI.Infrastructure;

/// <summary>
/// Shows the WPF save dialog and writes exported content. The dialog call is injectable so the
/// accept/cancel paths are testable without displaying a window.
/// </summary>
public class SaveFileDialogService(Func<string?>? showDialog) : ISaveFileDialogService
{
    private readonly Func<string?> _showDialog = showDialog ?? ShowWpfDialog;

    public SaveFileDialogService() : this(null)
    {
    }

    public string? ShowSaveFileDialog() => _showDialog();

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Modal OS dialog.")]
    private static string? ShowWpfDialog()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog();
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void SaveContent(string filePath, string content)
    {
        if (string.IsNullOrEmpty(filePath) == false)
        {
            System.IO.File.WriteAllText(filePath, content);
        }
    }
}
