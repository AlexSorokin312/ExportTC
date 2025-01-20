using ExportTC.Interfaces;
using ExportTC.Model.ElementParcers;
using HenconExport;

namespace ExportTC.Model.Factories
{
    public class ExcelReaderFactory : IExcelReaderFactory
    {
        public IExcelReader Create(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                LoggerDebug.LogError("Не найден путь к файлу Excel. Не могу создать ExcelReader.");
                return null;
            }

            LoggerDebug.LogInfo($"Создание ExcelReader для файла: {filePath}");
            var reader = new ExcelReader(filePath);
            LoggerDebug.LogDebug("ExcelReader успешно создан.");
            return reader;
        }
    }

    public class HtmReaderFactory : IHtmlReaderFactory
    {
        public IHtmlReader Create(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                LoggerDebug.LogError("Путь к файлу HTML равен null. Не могу создать HtmlReader.");
                return null;
            }

            LoggerDebug.LogInfo($"Создание HtmlReader для файла: {filePath}");
            var reader = new HtmlReader();
            LoggerDebug.LogDebug("HtmlReader успешно создан.");
            return reader;
        }
    }
}
