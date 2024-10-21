using HenconExport.Model.Elemnts;

public class Assembly
{

    private readonly List<Element> _elements;

    public Assembly(List<Element> elements)
    {
        _elements = elements ?? new List<Element>();
    }

    public IReadOnlyList<Element> Elements => _elements.AsReadOnly();

    public IEnumerable<Element> GetRootElements()
    {
        return _elements.Where(e => e.Parent == null); 
    }

    public void Sort()
    {
        foreach (var rootElement in GetRootElements())
        {
            SortChildren(rootElement);
        }
    }

    private void SortChildren(Element parent)
    {
        parent.Children = parent.Children?.OrderBy(e =>
        {
            int result;
            return int.TryParse(e.Designation, out result) ? result : int.MaxValue;
        }).ToList();

        if (parent.Children != null)
        {
            foreach (var child in parent.Children)
            {
                SortChildren(child);
            }
        }
    }
}