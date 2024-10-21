using ExportTC.Constants;
using ExportTC.Model;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using HenconExport.Model.HTMHandler;
using System.IO;

namespace HenconExport.Model.ExcelHandler
{
    public class ExcelDataExractor
    {
        private IExcelReader? _excelReader;
        private IHtmlHadnler _htmlHandler;
        private InitialData? _initialData;

        public ExcelDataExractor(IExcelReaderFactory exelReaderFactory, InitialData initialData, IHtmlReaderFactory htmReaderFactory)
        {
            _excelReader = exelReaderFactory.Create(initialData.ExcelFile);
            _htmlHandler = htmReaderFactory.Create(initialData.HtmlFile);
            _initialData = initialData;
        }

        public Assembly GetAllElements()
        {
            List<Element> elements = new List<Element>();
            try
            {
                var sheetNumber = _initialData.SheetNubmer;
                int startRow = _initialData.StartRow;
                int endRow = _initialData.EndRow;

                elements = GetAllElements(sheetNumber, startRow, endRow);
                DefineElementTypes(elements);


                var htmlStructure = _htmlHandler.GetElementsFromHTML(_initialData);

                JoinTwoCollections(elements, htmlStructure);

                var collection = new Assembly(elements);
                collection.Sort();



                return collection;

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Ошибка: файл не найден. {ex.Message}");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка: проблема с операцией. {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении данных из Excel: {ex.Message}");
                return null;
            }
        }

        private void JoinTwoCollections(List<Element> elements, List<TreeElementHTML> htmlElements)
        {
            foreach (var htmlElement in htmlElements)
            {
                var element = elements.FirstOrDefault(x => x.Designation == htmlElement.Designation);

                if (element == null)
                {
                    var parent = htmlElement.Parent;
                    var excelParent = elements.FirstOrDefault(x => x.Designation == parent.Designation);
                    element = new Element()
                    {
                        Quantity = htmlElement.Quantity,
                        Designation = htmlElement.Designation,
                        Parent = htmlElement.Parent,
                        Revision = htmlElement.Revision,
                        Name = htmlElement.Name,
                        Type = ElementConstants.PDF
                    };

                    htmlElement.MakeOrBuy = Constants.GetReplacment(htmlElement.MakeOrBuy);

                    if (string.IsNullOrEmpty(element.Name))
                        element.Name = "NoName";
                    excelParent.Children.Add(element);
                    elements.Add(element);
                }

                var allElemets = elements.Where(x => x.Designation == htmlElement.Designation).ToList();
                foreach (var copy in allElemets)
                {
                   // copy.FileName = htmlElement.FileName;
                }
            }
        }

        private List<Element> GetAllElements(int sheetNumber, int startRow, int lastRow)
        {
            var elements = new List<Element>();

            var assemblyElement = GetRootElement();

            for (int row = startRow; row <= lastRow; row++)
            {
                var element = CreateExcelElement(sheetNumber, row);
                if (element != null)
                    elements.Add(element);
            }

            AssignParentsAndChildren(elements);
            var elementsWithoutParents = elements.Where(e => e.Parent == null).ToList();
            elementsWithoutParents.ForEach(e =>e.Parent = assemblyElement);
            assemblyElement.Children.AddRange(elementsWithoutParents);

            elements.Insert(0, assemblyElement);

            return elements;
        }

        private Element CreateExcelElement(int sheetNumber, int row)
        {
            string pos = ReadCell(sheetNumber, _initialData.PositionColumn, row);
            string designation = ReadCell(sheetNumber, _initialData.DesignationColumn, row);
            string name = ReadCell(sheetNumber, _initialData.DescriptionColumn, row);
            string quantity = ReadCell(sheetNumber, _initialData.QuantityColumn, row);
            string makeOrBuy = ReadCell(sheetNumber, _initialData.MakeBuyColumn, row);
            string revision = ReadCell(sheetNumber, _initialData.RevisionColumn, row); 

            return new Element(designation, name, pos, quantity, makeOrBuy, revision);
        }

        private string ReadCell(int sheetNumber, string columnName, int row)
        {
            return _excelReader.ReadCell(sheetNumber, columnName, row);
        }

        private Element GetRootElement()
        {
            var sheetName = _initialData.SheetNubmer;
            string designation = _excelReader.ReadCell(sheetName, _initialData.ProductIDCell);
            string assemblyName = _excelReader.ReadCell(sheetName, _initialData.ProductNameCell);
            return new Element(designation, assemblyName);
        }

        private void AssignParentsAndChildren(List<Element> elements)
        {
            var childrenLookup = new Dictionary<string, List<Element>>();

            foreach (var element in elements)
            {
                if (element.Pos == null)
                    continue;
                string parentDesignation = GetParentDesignation(element.Pos);

                if (!string.IsNullOrEmpty(parentDesignation))
                {
                    var parent = elements.FirstOrDefault(x => x.Pos == parentDesignation);
                    if (parent != null)
                    {
                        element.Parent = parent; 
                                                
                        parent.Children ??= new List<Element>();
                        parent.Children.Add(element); 
                    }
                }

                if (!childrenLookup.ContainsKey(parentDesignation))
                {
                    childrenLookup[parentDesignation] = new List<Element>();
                }
                childrenLookup[parentDesignation].Add(element);
            }
        }

        private void DefineElementTypes(List<Element> elements)
        {
            foreach (var element in elements)
            {
                if (element.Children == null || element.Children.Count == 0)
                {
                    element.Type = ElementConstants.DETAIL;
                }
                else
                {
                    element.Type = ElementConstants.ASSEMBLY;
                }
            }
        }

        private string GetParentDesignation(string designation)
        {
            int lastIndex = designation.LastIndexOf('.');
            if (lastIndex > 0)
            {
                return designation.Substring(0, lastIndex);
            }
            return string.Empty;
        }

        public void Dispose() => _excelReader?.Dispose();

    }
}
