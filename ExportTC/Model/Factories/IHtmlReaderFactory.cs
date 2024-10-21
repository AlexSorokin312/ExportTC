using ExportTC.Interfaces;

namespace ExportTC.Model.Factories
{
    public interface IHtmlReaderFactory
    {
        IHtmlReader Create(string filePath);
    }
}