using OfficeOpenXml;
using System.IO;

namespace HenconExport
{
    public class ExcelWriter : IDisposable
    {
        private ExcelPackage _package;
        private bool _disposed = false;

        public ExcelWriter(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            _package = new ExcelPackage(fileInfo);
        }

        public void WriteCell(ExcelWorksheet worksheet, int row, int column, object value)
        {
            worksheet.Cells[row, column].Value = value;
        }

        public void Save()
        {
            _package.Save();
        }

        public ExcelWorksheet GetWorksheet(int index)
        {
            if (index < 1 || index > _package.Workbook.Worksheets.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс листа вне диапазона.");
            }

            return _package.Workbook.Worksheets[index - 1]; // Индексы начинаются с 0
        }

        public ExcelWorksheet GetWorksheet(string worksheetName)
        {
            return _package.Workbook.Worksheets[worksheetName]; // Индексы начинаются с 0
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

        ~ExcelWriter()
        {
            Dispose(false);
        }
    }
}