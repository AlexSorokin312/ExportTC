using ExportTC.Model;
using HenconExport.Model.Elemnts;

namespace HenconExport.Model.HTMHandler
{
    public interface IHtmlHadnler
    {
        public List<TreeElementHTML> GetElementsFromHTML(InitialData initialData);
    }
}