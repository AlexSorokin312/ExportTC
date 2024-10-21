namespace ExportTC.Services
{
    public interface IFileDialogService
    {
        string? OpenExcelFile();
        string? OpenHtmlFile();
        string? OpenDirectory();
    }
}
