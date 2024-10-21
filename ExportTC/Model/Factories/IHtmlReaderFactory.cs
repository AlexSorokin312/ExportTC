using HenconExport.Model.HTMHandler;

namespace ExportTC.Model.Factories
{
    public interface IHtmlReaderFactory
    {
        IHtmlHadnler Create(string filePath);
    }
}