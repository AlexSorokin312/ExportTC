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
                                             config.MakeOrBuyColumn, config.RevisionColumn,
                                             config.ItemCodeSupplier, config.Costtype,
                                             config.Spare, config.AddInfo);
            if (element != null)
            {
                elements.Add(element);
            }
        }

        //Проверка пизиций
        int quntityColumnNubmer = ColumnLetterToIndex(config.QuantityColumn);
        int descriptionColumnNumber = ColumnLetterToIndex(config.DescriptionColumn);
        int levels = descriptionColumnNumber - quntityColumnNubmer;

        int currentColumn = quntityColumnNubmer;

        int[] demensions = new int[levels];
        for (int i = 0; i < demensions.Length; i++)
        {
            demensions[i] = 1;
        }

        List<string> rows = new();
        int lastIndex = -1;
        for (int row = config.StartRow; row <= config.EndRow; row++)
        {
            string position = string.Empty;
            for (int i = 0; i < demensions.Length; i++)
            {
                bool found = false;

                var columnName = GetColumnName(currentColumn);
                currentColumn++;
                var value = excelReader.ReadCell(config.SheetNumber, columnName, row);

                if (string.IsNullOrEmpty(value))
                    continue;

                if (lastIndex == i)
                {
                    demensions[i] += 1;
                }
                if (i < lastIndex)
                {
                    demensions[i] += 1;
                    if (i +1  != demensions.Length)
                        demensions[i + 1] = 1;
                }

                for (int j = 0; j <= i; j++)
                {
                    if (demensions[j] / 10 == 0)
                        position += string.Format("00{0}.", demensions[j]);
                    else
                         position += string.Format("0{0}.", demensions[j]);

                    currentColumn = quntityColumnNubmer;
                    found = true;
                    lastIndex = j;
                }
                rows.Add(position);
                if (!string.IsNullOrEmpty(position))
                     position = position.Substring(0, position.Length - 1);

                string designation = excelReader.ReadCell(config.SheetNumber, config.DesignationColumn, row);
                var element = elements.FirstOrDefault(e => e.Designation == designation && string.IsNullOrEmpty(e.Pos));
                
                element.Pos = position;
                if (!element.Pos.Equals(position))
                {

                }

                if (found)
                    break;
                
            }
        }
        
        AssignParentsAndChildren(elements);
        AddElementsWithoutParentsToRoot(elements, rootElement);
        elements[0].Root = true;
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
                                       string revisionColumn,
                                       string itemCodeSupplierColumn,
                                       string costtypeColumn,
                                       string spareColumn,
                                       string addInfoColumn)
    {
        //string pos = excelReader.ReadCell(sheetNumber, positionColumn, row) ?? string.Empty;
        string pos =  string.Empty;
        string designation = excelReader.ReadCell(sheetNumber, designationColumn, row) ?? string.Empty;
        string name = excelReader.ReadCell(sheetNumber, descriptionColumn, row) ?? string.Empty;
        string quantity = excelReader.ReadCell(sheetNumber, quantityColumn, row) ?? string.Empty;
        string makeOrBuy = excelReader.ReadCell(sheetNumber, makeOrBuyColumn, row) ?? string.Empty;
        string revision = excelReader.ReadCell(sheetNumber, revisionColumn, row) ?? string.Empty;

        // Чтение новых столбцов
        string itemCodeSupplier = excelReader.ReadCell(sheetNumber, itemCodeSupplierColumn, row) ?? string.Empty;
        string costtype = excelReader.ReadCell(sheetNumber, costtypeColumn, row) ?? string.Empty;
        string spare = excelReader.ReadCell(sheetNumber, spareColumn, row) ?? string.Empty;
        string addInfo = excelReader.ReadCell(sheetNumber, addInfoColumn, row) ?? string.Empty;

        // Создание элемента с новыми полями
        var element = new Element(designation, name, pos, quantity, makeOrBuy, revision)
        {
            ItemCodeSupplier = itemCodeSupplier,
            Costtype = costtype,
            Spare = spare,
            AddInfo = addInfo
        };

        return element;
    }


    private void AssignParentsAndChildren(List<Element> elements)
    {
        // Убираем дубликаты
        var parentLookup = elements.Where(e => !string.IsNullOrEmpty(e.Pos))
                                   .GroupBy(e => e.Pos)
                                   .ToDictionary(g => g.Key, g => g.First());

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
        elements.Insert(0, rootElement); 
    }

    private string GetParentDesignation(string pos)
    {
        int lastDotIndex = pos.LastIndexOf('.');
        return (lastDotIndex > 0) ? pos.Substring(0, lastDotIndex) : string.Empty;
    }


    private int ColumnLetterToIndex(string columnLetter)
    {
        int index = 0;
        foreach (char c in columnLetter.ToUpper())
        {
            index *= 26;
            index += c - 'A' + 1;
        }
        return index;
    }

    private string GetColumnName(int columnIndex)
    {
        int div = columnIndex;
        string columnName = "";
        while (div >= 0)
        {
            int mod = div % 26;
            columnName = Convert.ToChar(mod + 65) + columnName;
            div = (div / 26) - 1;
        }
        return columnName;
    }
}
