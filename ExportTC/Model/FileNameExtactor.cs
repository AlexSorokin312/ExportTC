using System.IO;

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
}
