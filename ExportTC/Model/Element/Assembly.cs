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
        if (_elements.Count <= 1) return; 

        var firstElement = _elements[0];

        var sortedElements = _elements.Skip(1)
            .Where(e => e.Pos != null) 
            .ToList();

        sortedElements.Sort((a, b) =>
        {
            int aDotCount = CountDots(a.Pos);
            int bDotCount = CountDots(b.Pos);

            if (aDotCount != bDotCount) return aDotCount.CompareTo(bDotCount);

            return CompareDesignation(a.Designation, b.Designation);
        });

        var nullElements = _elements.Skip(1)
            .Where(e => e.Pos == null)
            .ToList();

        sortedElements.Insert(0, firstElement);
        sortedElements.AddRange(nullElements);

        _elements.Clear();
        _elements.AddRange(sortedElements);
    }

    private int CountDots(string? pos)
    {
        return pos?.Count(c => c == '.') ?? 0; // Считаем количество точек
    }

    private int CompareDesignation(string? designationA, string? designationB)
    {
        if (designationA == null && designationB == null) return 0;
        if (designationA == null) return 1; // null в конце
        if (designationB == null) return -1; // null в конце

        // Пробуем преобразовать в числа
        bool isNumberA = int.TryParse(designationA, out int numA);
        bool isNumberB = int.TryParse(designationB, out int numB);

        if (isNumberA && isNumberB) return numA.CompareTo(numB);
        if (isNumberA) return -1; // Число впереди
        if (isNumberB) return 1; // Число впереди

        // Если оба не числа, сравниваем строки
        return string.Compare(designationA, designationB, StringComparison.Ordinal);
    }
}
