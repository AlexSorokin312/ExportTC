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

    public string? FindFileInSubdirectories(string directory, string fileName)
    {
        try
        {
            return Directory.EnumerateFiles(directory, fileName, SearchOption.AllDirectories).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске файла: {ex.Message}");
            return null;
        }
    }
}