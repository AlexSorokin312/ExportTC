using HenconExport;

namespace ExportTC.Model.Factories
{
    public interface IExcelReaderFactory
    {
        public IExcelReader Create(string filePath);
    }
}