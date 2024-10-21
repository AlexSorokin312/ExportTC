using HenconExport.Model.Elemnts;

namespace ExportTC.Interfaces
{
    public interface IHtmlReader
    {
        void FillDataFromHtml(string htmlPath, List<Element> elements);
    }
}
