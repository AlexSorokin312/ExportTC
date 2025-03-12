using ExportTC.Constants;
using HenconExport.Model.Elemnts;
using System.Xml.Linq;

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
        var root = _elements.Where(x => x.Designation == RootAssembly.value);
        if (root == null)
            return _elements.Where(e => e.Parent == null);
        return root;
    }

    public Element GetRootElement()
    {
        var root = _elements.FirstOrDefault(x => x.Designation == RootAssembly.value);
        if (root != null)
            return root;
        else
            return _elements.FirstOrDefault(e => e.Parent == null);
        
    }

    public void Sort()
    {
        if (_elements.Count <= 1) return; 

        var firstElement1= _elements.Where(x=>x.Children.Count != 0);
        var firstElement = _elements.FirstOrDefault(x=>x.Children.Count != 0);
        firstElement.Parent = null;

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

        var assemblyes = sortedElements.Where(e => e.DrawingIcon == ElementConstants.ASSEMBLY).ToList();
        var rest = sortedElements.Where(e => e.DrawingIcon != ElementConstants.ASSEMBLY).ToList();
        assemblyes.AddRange(rest);

        SetQuantityForFullyEqualElements(assemblyes);
        //SetQuantityForElementsWithDifferentFiles(assemblyes);
        RemoveFullyEqualElements(assemblyes);

        _elements.Clear();
        _elements.AddRange(assemblyes);
    }

    private void SetQuantityForElementsWithDifferentFiles(List<Element> elements)
    {
        var groupedElements = elements
            .GroupBy(e => new
            {
                e.Designation,
                e.Quantity,
                e.Name,
                e.Revision,
                ParentDesignation = e.Parent?.Designation, // Родительский элемент
            });

        foreach (var group in groupedElements)
        {
            bool isFirst = true;
            foreach (var element in group)
            {
                if (element.Designation == "633466100")
                {

                }
                if (isFirst)
                {
                    isFirst = false; // Первый элемент оставляем без изменений
                }
                else
                {
                    element.Quantity = "0"; // Всем остальным присваиваем "0"
                }
            }
        }
    }

    private void SetQuantityForFullyEqualElements(List<Element> elements)
    {
        var groupedElements = elements
            .GroupBy(e => new
            {
                e.Pos,
                e.Designation,
                e.Quantity,
                e.Name,
                e.MakeOrBuy,
                e.ProductStatus,
                e.Revision,
                e.TCType,
                e.DrawingFile,
                e.EADrawingFile,
                e.REDrawingFile,
                e.EMDrawingFile,
                e.ZipFile,
                e.STEP,
                e.PPT,
                e.PPTX,
                e.TXT,
                e.BMP,
                e.MSG,
                e.Multy,
                e.PartFile,
                e.AssemblyFile,
                e.DocFile,
                e.DocxFile,
                e.JpegFile,
                e.PDFFile,
                e.Html,
                e.DWG,
                e.DXF,
                e.GIF,
                e.TIF,
                e.PNG,
                e.FileName,
                e.DrawingIcon,
                ParentDesignation = e.Parent?.Designation, // Родительский элемент
                e.Root,
                e.ItemCodeSupplier,
                e.Costtype,
                e.Spare,
                e.AddInfo,
                e.ExcelFile
            });

        foreach (var group in groupedElements)
        {
            bool isFirst = true;
            foreach (var element in group)
            {
                if (isFirst)
                {
                    isFirst = false; // Первый элемент оставляем без изменений
                }
                else
                {
                    element.Quantity = "0"; // Всем остальным присваиваем "0"

                }
            }
        }
    }

    private void RemoveFullyEqualElements(List<Element> elements)
    {
        var uniqueElements = elements
            .GroupBy(e => new
            {
                e.Pos,
                e.Designation,
                e.Name,
                e.MakeOrBuy,
                e.ProductStatus,
                e.Revision,
                e.TCType,
                e.DrawingFile,
                e.EADrawingFile,
                e.REDrawingFile,
                e.EMDrawingFile,
                e.ZipFile,
                e.STEP,
                e.PPT,
                e.PPTX,
                e.TXT,
                e.BMP,
                e.MSG,
                e.Multy,
                e.PartFile,
                e.AssemblyFile,
                e.DocFile,
                e.DocxFile,
                e.JpegFile,
                e.PDFFile,
                e.Html,
                e.DWG,
                e.DXF,
                e.GIF,
                e.TIF,
                e.PNG,
                e.FileName,
                e.DrawingIcon,
                ParentDesignation = e.Parent?.Designation, // Родительский элемент
                e.Root,
                e.ItemCodeSupplier,
                e.Costtype,
                e.Spare,
                e.AddInfo,
                e.ExcelFile
            })
            .Select(g => g.First())
            .ToList();

        // Очищаем исходный список и добавляем только уникальные элементы
        elements.Clear();
        elements.AddRange(uniqueElements);
    }

    /// <summary>
    /// Рекурсивно устанавливает Quantity = "0" для всех дочерних элементов.
    /// </summary>
    private void SetChildrenQuantityToZero(Element element)
    {
        if (element.Children != null)
        {
            foreach (var child in element.Children)
            {
                if (child.Designation == "447018733")
                {

                }
                child.Quantity = "0";
                SetChildrenQuantityToZero(child); // Рекурсивный вызов для всех уровней вложенности
            }
        }
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
