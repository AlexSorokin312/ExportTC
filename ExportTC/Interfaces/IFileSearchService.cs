public interface IFileSearchService
{
    string? FindFirstExcelFile(string directoryPath);
    string? FindHtmlFile(string directoryPath);
    string? FindFileInSubdirectories(string directoryPath, string name);
    IEnumerable<string> FindFilesWithCriteria(string directory, string searchString, string extension);
}
