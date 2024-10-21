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
        // Сортируем детей текущего элемента по Designation
        parent.Children = parent.Children?.OrderBy(e =>
        {
            // Пробуем разобрать Designation как число для корректной сортировки
            int result;
            return int.TryParse(e.Designation, out result) ? result : int.MaxValue;
        }).ToList();

        // Рекурсивно сортируем детей каждого элемента
        if (parent.Children != null)
        {
            foreach (var child in parent.Children)
            {
                SortChildren(child);
            }
        }
    }
}