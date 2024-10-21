using ExportTC.Model.Factories;
using HenconExport;
using HenconExport.Model.Elemnts;

public class ExcelElementParser
{
    private readonly IExcelReaderFactory _excelReaderFactory;

    public ExcelElementParser(IExcelReaderFactory excelReaderFactory)
    {
        _excelReaderFactory = excelReaderFactory;
    }

    public List<Element> GetExcelElements(ExcelElementConfig config)
    {
        var excelReader = _excelReaderFactory.Create(config.ExcelPath)
                          ?? throw new ArgumentException("Failed to create Excel reader.");

        return ExtractElementsFromExcel(excelReader, config);
    }

    private List<Element> ExtractElementsFromExcel(IExcelReader excelReader, ExcelElementConfig config)
    {
        var elements = new List<Element>();
        var rootElement = CreateRootElement(excelReader, config.SheetNumber, config.ProductIDCell, config.ProductNameCell);

        for (int row = config.StartRow; row <= config.EndRow; row++)
        {
            var element = CreateExcelElement(excelReader, config.SheetNumber, row,
                                             config.PositionColumn, config.DesignationColumn,
                                             config.DescriptionColumn, config.QuantityColumn,
                                             config.MakeOrBuyColumn, config.RevisionColumn);
            if (element != null)
            {
                elements.Add(element);
            }
        }

        AssignParentsAndChildren(elements);
        AddElementsWithoutParentsToRoot(elements, rootElement);

        return elements;
    }

    private Element CreateRootElement(IExcelReader excelReader, int sheetNumber, string productIDCell, string productNameCell)
    {
        string designation = excelReader.ReadCell(sheetNumber, productIDCell) ?? string.Empty;
        string assemblyName = excelReader.ReadCell(sheetNumber, productNameCell) ?? string.Empty;
        return new Element(designation, assemblyName);
    }

    private Element CreateExcelElement(IExcelReader excelReader,
                                       int sheetNumber,
                                       int row,
                                       string positionColumn,
                                       string designationColumn,
                                       string descriptionColumn,
                                       string quantityColumn,
                                       string makeOrBuyColumn,
                                       string revisionColumn)
    {
        string pos = excelReader.ReadCell(sheetNumber, positionColumn, row) ?? string.Empty;
        string designation = excelReader.ReadCell(sheetNumber, designationColumn, row) ?? string.Empty;
        string name = excelReader.ReadCell(sheetNumber, descriptionColumn, row) ?? string.Empty;
        string quantity = excelReader.ReadCell(sheetNumber, quantityColumn, row) ?? string.Empty;
        string makeOrBuy = excelReader.ReadCell(sheetNumber, makeOrBuyColumn, row) ?? string.Empty;
        string revision = excelReader.ReadCell(sheetNumber, revisionColumn, row) ?? string.Empty;

        return new Element(designation, name, pos, quantity, makeOrBuy, revision);
    }

    private void AssignParentsAndChildren(List<Element> elements)
    {
        var parentLookup = elements.ToDictionary(e => e.Pos, e => e);

        foreach (var element in elements)
        {
            if (!string.IsNullOrEmpty(element.Pos))
            {
                string parentPos = GetParentDesignation(element.Pos);
                if (parentLookup.TryGetValue(parentPos, out var parent))
                {
                    element.Parent = parent;
                    parent.Children.Add(element);
                }
            }
        }
    }

    private void AddElementsWithoutParentsToRoot(List<Element> elements, Element rootElement)
    {
        var orphans = elements.Where(e => e.Parent == null).ToList();
        orphans.ForEach(e => e.Parent = rootElement);
        rootElement.Children.AddRange(orphans);
        elements.Insert(0, rootElement); // Root element at the beginning
    }

    private string GetParentDesignation(string pos)
    {
        int lastDotIndex = pos.LastIndexOf('.');
        return (lastDotIndex > 0) ? pos.Substring(0, lastDotIndex) : string.Empty;
    }
}
