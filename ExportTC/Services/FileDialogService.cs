using ExportTC.Services;
using System.Windows.Forms;

public class FileDialogService : IFileDialogService
{
    public string? OpenExcelFile()
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
    }

    public string? OpenHtmlFile()
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Filter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
    }

    public string? OpenDirectory()
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Выберите папку";
            dialog.ShowNewFolderButton = true;
            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }
    }
}