using ExportTC.Interfaces;
using HenconExport.Model.Elemnts;

namespace ExportTC.Model.ElementParcers
{
    public class HtmlElementParser
    {
        private IHtmlReader _reader; 
        private IElementTreeBuilder _treeBuilder;

        public HtmlElementParser(IHtmlReader reader, IElementTreeBuilder treeBuilder)
        {
            _reader = reader;
            _treeBuilder = treeBuilder;
        }

        public List<Element> GetElementsFromHTML(string htmlPath)
        {
            var allElements = _treeBuilder.BuildTreeWithParents(htmlPath);
            _reader.FillDataFromHtml(htmlPath, allElements);
           
            return allElements;
        }


        private void UpdateElementData(Element treeElement, List<Element> allElements)
        {
            foreach (var matchingElement in allElements)
            {
                if (matchingElement.Designation == treeElement.Designation)
                {
                    matchingElement.Parent = treeElement.Parent;
                }
            }

            foreach (var child in treeElement.Children)
            {
                UpdateElementData(child, allElements);
            }
        }
    }
}
