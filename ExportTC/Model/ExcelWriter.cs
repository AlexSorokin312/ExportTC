using OfficeOpenXml;
using System;
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

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Файл не найден.", filePath);
            }

            _package = new ExcelPackage(fileInfo);
        }

        // Запись в ячейку с проверкой на тип данных
        public void WriteCell(ExcelWorksheet worksheet, int row, int column, object value)
        {
            if (worksheet == null) throw new ArgumentNullException(nameof(worksheet), "Рабочий лист не может быть null.");
            if (row < 1 || column < 1) throw new ArgumentOutOfRangeException("Номер строки и столбца должен быть больше 0.");

            worksheet.Cells[row, column].Value = value ?? string.Empty; // Защита от записи null
        }

        // Сохранение данных в файл
        public void Save()
        {
            try
            {
                _package.Save();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при сохранении Excel файла.", ex);
            }
        }

        // Получение рабочего листа по индексу с проверкой
        public ExcelWorksheet GetWorksheet(int index)
        {
            if (index < 1 || index > _package.Workbook.Worksheets.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс листа вне диапазона.");
            }

            return _package.Workbook.Worksheets[index - 1];
        }

        // Получение рабочего листа по имени с проверкой
        public ExcelWorksheet GetWorksheet(string worksheetName)
        {
            if (string.IsNullOrEmpty(worksheetName))
            {
                throw new ArgumentException("Имя рабочего листа не может быть пустым.", nameof(worksheetName));
            }

            var worksheet = _package.Workbook.Worksheets[worksheetName];
            if (worksheet == null)
            {
                throw new ArgumentException($"Лист с именем '{worksheetName}' не найден.", nameof(worksheetName));
            }

            return worksheet;
        }

        // Реализация IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Основная логика для освобождения ресурсов
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
