using HenconExport;
using HenconExport.Model.HTMHandler;

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
        public IHtmlHadnler Create(string filePath)
        {
            if (filePath == null)
                return null;
            var reader = new HtmlHandler(filePath);
            return reader;
        }
    }
}
