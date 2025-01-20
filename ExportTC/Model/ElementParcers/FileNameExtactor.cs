using System.IO;
using System.Text.RegularExpressions;

public static class FileNameExtactor
{
    public static string ExtractFileNameFromText(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string fileContent = File.ReadAllText(filePath);

        string searchPattern = "FILE_NAME";
        int startIndex = fileContent.IndexOf(searchPattern);

        if (startIndex != -1)
        {
            startIndex = fileContent.IndexOf("<TD VALIGN=middle>", startIndex) + "<TD VALIGN=middle>".Length;

            int endIndex = fileContent.IndexOf("</TD>", startIndex);

            if (startIndex != -1 && endIndex != -1)
            {
                string fileName = fileContent.Substring(startIndex, endIndex - startIndex).Trim();
                return fileName;
            }
        }
        return null;
    }

    public static string ExtractDesignation(string innerHtml)
    {
        var match = Regex.Match(innerHtml, @"<a.*?href=""\d+\.htm"".*?>(\d+)<\/a>");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }

    public static string ExtractHrefValueFromColumn(string innerHtml, string htmlPath)
    {
        var match = Regex.Match(innerHtml, @"href=""(\d+\.htm)""");
        var result = match.Success ? match.Groups[1].Value : null;

        if (result == null)
            return null;

        string directory = Path.GetDirectoryName(htmlPath);

        string foundFilePath = FindFileInSubdirectories(directory, result);
        string extractedFileName = FileNameExtactor.ExtractFileNameFromText(foundFilePath);
        return extractedFileName;
    }

    public static string FindFileInSubdirectories(string directory, string fileName)
    {
        try
        {
            var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
            return files.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске файла: {ex.Message}");
            return null;
        }
    }
}
