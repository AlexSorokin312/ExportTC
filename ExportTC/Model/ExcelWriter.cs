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

            if (!fileInfo.Exists)
            {
                LoggerDebug.LogError($"Файл не найден: {filePath}");
                throw new FileNotFoundException("Файл не найден.", filePath);
            }

            LoggerDebug.LogInfo($"Инициализация ExcelWriter для файла: {filePath}");
            _package = new ExcelPackage(fileInfo);
        }

        // Запись в ячейку с проверкой на тип данных
        public void WriteCell(ExcelWorksheet worksheet, int row, int column, object value)
        {
            if (worksheet == null)
            {
                LoggerDebug.LogError("Рабочий лист не может быть null.");
                throw new ArgumentNullException(nameof(worksheet), "Рабочий лист не может быть null.");
            }
            if (row < 1 || column < 1)
            {
                LoggerDebug.LogError("Номер строки и столбца должен быть больше 0.");
                throw new ArgumentOutOfRangeException("Номер строки и столбца должен быть больше 0.");
            }

            worksheet.Cells[row, column].Value = value ?? string.Empty;
        }

        public void Save()
        {
            try
            {
                LoggerDebug.LogInfo("Сохранение Excel файла.");
                _package.Save();
                LoggerDebug.LogInfo("Excel файл успешно сохранён.");
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при сохранении Excel файла: {ex.Message}");
                throw new InvalidOperationException("Ошибка при сохранении Excel файла.", ex);
            }
        }

        // Получение рабочего листа по индексу с проверкой
        public ExcelWorksheet GetWorksheet(int index)
        {
            if (index < 1 || index > _package.Workbook.Worksheets.Count)
            {
                LoggerDebug.LogError($"Индекс листа вне диапазона: {index}");
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс листа вне диапазона.");
            }

            LoggerDebug.LogInfo($"Получение рабочего листа по индексу: {index}");
            return _package.Workbook.Worksheets[index - 1];
        }

        // Получение рабочего листа по имени с проверкой
        public ExcelWorksheet GetWorksheet(string worksheetName)
        {
            if (string.IsNullOrEmpty(worksheetName))
            {
                LoggerDebug.LogError("Имя рабочего листа не может быть пустым.");
                throw new ArgumentException("Имя рабочего листа не может быть пустым.", nameof(worksheetName));
            }

            var worksheet = _package.Workbook.Worksheets[worksheetName];
            if (worksheet == null)
            {
                LoggerDebug.LogError($"Лист с именем '{worksheetName}' не найден.");
                throw new ArgumentException($"Лист с именем '{worksheetName}' не найден.", nameof(worksheetName));
            }

            LoggerDebug.LogInfo($"Получение рабочего листа по имени: {worksheetName}");
            return worksheet;
        }

        public void Dispose()
        {
            LoggerDebug.LogInfo("Освобождение ресурсов ExcelWriter.");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    LoggerDebug.LogInfo("Закрытие ExcelPackage.");
                    _package?.Dispose();
                }

                _disposed = true;
                LoggerDebug.LogInfo("Ресурсы ExcelWriter успешно освобождены.");
            }
        }

        ~ExcelWriter()
        {
            LoggerDebug.LogWarning("Деструктор ExcelWriter вызван. Неявное освобождение ресурсов.");
            Dispose(false);
        }
    }
}
