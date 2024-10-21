using HenconExport.Model.Elemnts;

namespace ExportTC.Interfaces
{
    public interface IElementTreeBuilder
    {
        List<Element> BuildTreeWithParents(string htmlPath);
    }
}
