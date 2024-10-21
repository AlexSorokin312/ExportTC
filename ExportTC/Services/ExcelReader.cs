using OfficeOpenXml;
using System.IO;

namespace HenconExport
{
    public class ExcelReader : IDisposable, IExcelReader
    {
        private ExcelPackage _package;
        private bool _disposed = false;

        public ExcelReader(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            _package = new ExcelPackage(fileInfo);
        }

        public List<string> ReadColumnFromRow(string worksheetName, string columnName, int startRow)
        {
            var lastRow = GetLastUsedRow(worksheetName, columnName, startRow);
            return ReadColumnRange(worksheetName, columnName, startRow, lastRow);
        }

        public List<string> ReadIntColumnFromRow(string worksheetName, string columnName, int startRow)
        {
            var lastRow = GetLastUsedRow(worksheetName, columnName, startRow);
            var stringValues = ReadColumnRange(worksheetName, columnName, startRow, lastRow);
            var intValues = new List<string>();

            for (int i = 0; i < stringValues.Count; i++)
            {
                intValues.Add(stringValues[i]);
            }

            return intValues;
        }

        public List<string> ReadColumnRange(string worksheetName, string columnName, int startRow, int endRow)
        {
            try
            {
                var worksheet = GetWorksheet(worksheetName);
                var rangeAddress = $"{columnName}{startRow}:{columnName}{endRow}";
                var range = worksheet.Cells[rangeAddress];

                var values = new List<string>();

                foreach (var cell in range)
                {
                    if (cell.Value != null)
                    {
                        var value = cell.Value.ToString();
                        values.Add(value);
                    }

                    else
                        values.Add(string.Empty);
                }

                return values;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private ExcelWorksheet GetWorksheet(string worksheetName)
        {
            var worksheet = _package.Workbook.Worksheets[worksheetName];
            if (worksheet == null)
                throw new ArgumentException($"Лист с названием '{worksheetName}' не найден.", nameof(worksheetName));

            return worksheet;
        }

        public string ReadCell(string worksheetName, string columnName, int row)
        {
            var worksheet = GetWorksheet(worksheetName);
            var cell = worksheet.Cells[$"{columnName}{row}"];

            if (cell?.Value != null)
                return cell.Value.ToString();

            return string.Empty;
        }

        public string ReadCell(string worksheetName, string cellAddress)
        {
            var worksheet = GetWorksheet(worksheetName);
            var cell = worksheet.Cells[cellAddress];

            if (cell?.Value != null)
                return cell.Value.ToString();

            return string.Empty;
        }


        public int GetLastUsedRow(string worksheetName, string columnName, int startRow)
        {
            var worksheet = GetWorksheet(worksheetName);
            return GetLastUsedRowInWorksheet(worksheet, columnName, startRow);
        }

        public int GetLastUsedRow(int sheetIndex, string columnName, int startRow)
        {
            var worksheet = GetWorksheet(sheetIndex);
            return GetLastUsedRowInWorksheet(worksheet, columnName, startRow);
        }

        private int GetLastUsedRowInWorksheet(ExcelWorksheet worksheet, string columnName, int startRow)
        {
            if (worksheet.Dimension == null)
                return 0;

            int lastUsedRow = worksheet.Dimension.End.Row;

            // Перебираем строки начиная с startRow
            for (int row = startRow; row <= lastUsedRow; row++)
            {
                var cellValue = worksheet.Cells[$"{columnName}{row}"].Value;
                // Если наткнулись на первую пустую строку, возвращаем предыдущую строку как последнюю используемую
                if (cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString()))
                {
                    return row - 1;
                }
            }

            // Если не встретили пустых строк, возвращаем последнюю заполненную строку
            return lastUsedRow;
        }

        private ExcelWorksheet GetWorksheet(int sheetIndex)
        {
            if (sheetIndex < 0)
                throw new ArgumentOutOfRangeException();
            var count = _package.Workbook.Worksheets.Count;
            if (sheetIndex > count - 1)
                throw new ArgumentOutOfRangeException();

            return _package.Workbook.Worksheets[sheetIndex];
        }

        public string ReadCell(int sheetIndex, string cellAddress)
        {
            var worksheet = GetWorksheet(sheetIndex);
            var cell = worksheet.Cells[cellAddress];

            if (cell?.Value != null)
                return cell.Value.ToString();

            return string.Empty;
        }

        public string ReadCell(int sheetIndex, string columnName, int row)
        {
            var worksheet = GetWorksheet(sheetIndex);
            var cell = worksheet.Cells[$"{columnName}{row}"];

            if (cell?.Value != null)
                return cell.Value.ToString();

            return string.Empty;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _package?.Dispose();
                }
                _disposed = true;
            }
        }

        ~ExcelReader()
        {
            Dispose(false);
        }
    }
}
