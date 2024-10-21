namespace HenconExport
{
    public interface IExcelReader : IDisposable
    {
        List<string> ReadColumnFromRow(string worksheetName, string columnName, int startRow);
        List<string> ReadIntColumnFromRow(string worksheetName, string columnName, int startRow);
        List<string> ReadColumnRange(string worksheetName, string columnName, int startRow, int endRow);
        string ReadCell(string worksheetName, string columnName, int row);
        string ReadCell(string worksheetName, string cellAddress);
        int GetLastUsedRow(string worksheetName, string columnName, int startRow);
        int GetLastUsedRow(int sheetIndex, string columnName, int startRow);
        string ReadCell(int sheetIndex, string cellAddress);
        string ReadCell(int sheetIndex, string columnName, int row);
    }
}
