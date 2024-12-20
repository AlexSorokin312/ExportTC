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
        string pos = excelReader.ReadCell(sheetNumber, positionColumn, row) ?? string.Empty;
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


    //<------------------------------------------------------------------------------------------>
    private string FillPos(IExcelReader excelReader, int row, string quantityColumnName, string designationColumnName, int sheetNumber)
    {
        // Переменная для хранения результата
        string result = "";

        // Получаем индекс столбца, в котором находится имя столбца с количеством
        int quantityColumnIndex = GetColumnIndex(quantityColumnName);
        if (quantityColumnIndex == -1)
        {
            // Если столбец с таким именем не найден, выходим
            Console.WriteLine($"Столбец с именем {quantityColumnName} не найден.");
            return string.Empty;
        }

        // Чтение ячейки, которая соответствует столбцу с количеством
        var quantityCellValue = excelReader.ReadCell(sheetNumber, GetColumnName(quantityColumnIndex), row);

        // Преобразуем значение в число
        if (!int.TryParse(quantityCellValue, out int quantity))
        {
            // Если значение не число, то что-то пошло не так
            Console.WriteLine("Неверное значение в ячейке количества");
            return string.Empty;
        }

        // Начинаем считывать значения ячеек начиная с позиции, которая идет после столбца с количеством
        int currentColumnIndex = quantityColumnIndex + 1; // Следующий столбец после quantityColumnName
        string cellValue = "";

        // Читаем ячейки, пока не найдем нужное условие
        while (true)
        {
            cellValue = excelReader.ReadCell(sheetNumber, GetColumnName(currentColumnIndex), row);

            // Если ячейка содержит текст (например, буквы), прекращаем цикл
            if (string.IsNullOrEmpty(cellValue))
            {
                currentColumnIndex++; 
                continue;
            }

            // Если это число (проверяем по содержимому), и ищем родительскую позицию
            if (int.TryParse(cellValue, out int number))
            {
                result += cellValue; // Сохраняем число
                int parentValue;
                currentColumnIndex--; // Переходим на один столбец назад

                // Читаем вверх по этому столбцу, пока не найдем еще одно число
                while (row > 1) // Пока не достигнем первой строки
                {
                    row--; // Двигаемся вверх
                    string previousCellValue = excelReader.ReadCell(sheetNumber, GetColumnName(currentColumnIndex), row);

                    // Если ячейка пустая или содержит текст, пропускаем ее
                    if (string.IsNullOrEmpty(previousCellValue) || previousCellValue.All(char.IsLetter))
                    {
                        continue;
                    }

                    // Если это число, сохраняем и выходим
                    if (int.TryParse(previousCellValue, out int previousNumber))
                    {
                        string designationValue = excelReader.ReadCell(sheetNumber, designationColumnName, row);
                        result += " " + designationValue; // Добавляем значение из столбца designationColumnName

                        break; // Останавливаем цикл
                    }
                }

                break; // Останавливаем основной цикл, так как все найдено

            }

            if (!string.IsNullOrEmpty(cellValue))
            {
                return string.Empty;
            }

            // Если ячейка не число и не текст, продолжаем читать следующую ячейку
            currentColumnIndex++; // Переходим к следующему столбцу
        }

        return result;
    }

    // Метод для получения индекса столбца по его имени (например, A -> 1, B -> 2)
    private int GetColumnIndex(string columnName)
    {
        int columnIndex = 0;
        int factor = 1;

        // Преобразуем строковое имя в индекс (например, A -> 1, B -> 2)
        for (int i = columnName.Length - 1; i >= 0; i--)
        {
            columnIndex += (columnName[i] - 'A' + 1) * factor;
            factor *= 26;
        }

        return columnIndex - 1; // Возвращаем индекс с 0
    }

    // Метод для получения буквы столбца по его индексу
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
