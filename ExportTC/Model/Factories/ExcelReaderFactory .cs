using ExportTC.Interfaces;
using ExportTC.Model.ElementParcers;
using HenconExport;

namespace ExportTC.Model.Factories
{
    public class ExcelReaderFactory : IExcelReaderFactory
    {
        public IExcelReader Create(string filePath)
        {
            if (filePath == null)
                return null;
            var reader = new ExcelReader(filePath);
            return reader;
        }
    }

    public class HtmReaderFactory : IHtmlReaderFactory
    {
        public IHtmlReader Create(string filePath)
        {
            if (filePath == null)
                return null;
            var reader = new HtmlReader();
            return reader;
        }
    }
}
