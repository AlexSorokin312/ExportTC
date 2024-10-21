using System.IO;

public class FileSearchService : IFileSearchService
{
    public string? FindFirstExcelFile(string directoryPath)
    {
        return Directory.EnumerateFiles(directoryPath, "*.xls*", SearchOption.AllDirectories).FirstOrDefault();
    }

    public string? FindHtmlFile(string directoryPath)
    {
        return Directory.EnumerateFiles(directoryPath, "tree.htm", SearchOption.AllDirectories).FirstOrDefault();
    }
}