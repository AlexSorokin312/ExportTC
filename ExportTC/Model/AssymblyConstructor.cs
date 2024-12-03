using ExportTC.Constants;
using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Text.RegularExpressions;

namespace ExportTC.Model
{
    internal class AssemblyConstructor
    {
        private readonly ExcelElementParser _excelElementParser;
        private readonly HtmlElementParser _htmlElementParcer;
        private readonly IFileSearchService _fileSearchService;
        private readonly IExcelReaderFactory _excelFactory;
        private readonly ParametersDefinder _parametersDefinder;

        public AssemblyConstructor(ExcelElementParser excelElementParser,
            HtmlElementParser htmlElementParcer,
            IExcelReaderFactory excelFactory,
            IFileSearchService fileSearchService)
        {
            _htmlElementParcer = htmlElementParcer;
            _excelElementParser = excelElementParser;
            _excelFactory = excelFactory;
            _fileSearchService = fileSearchService;

            _parametersDefinder = new ParametersDefinder();
        }

        public Assembly GetAssembly(InitialData initialData)
        {
            if (initialData == null)
                throw new ArgumentNullException(nameof(initialData));

            var excelElements = GetDataFromExcel(initialData);
            var htmlElements = GetDataFromHtml(initialData);

            var filteredElements = htmlElements
                .Where(e => !Regex.IsMatch(e.Designation, @"\b[А-ЯA-Z]\d+\b", RegexOptions.IgnoreCase))
                .ToList();

            filteredElements.FirstOrDefault().Parent = null;

            MergeExcelElementsWithHtmlData(excelElements, filteredElements);
            MatchQuantity(filteredElements, excelElements);
            MakeAdditionalParamters(filteredElements, excelElements);
            FillFileNames(filteredElements, initialData.BaseDirectory);
            LinkDocumentsToDetails(filteredElements);
            FindFiles(filteredElements, initialData);
            var assembly = new Assembly(filteredElements);
            assembly.Sort();
            return assembly;
        }

        private void MergeExcelElementsWithHtmlData(List<Element> elements, List<Element> htmlElements)
        {
            foreach (var htmlElement in htmlElements)
            {
                var element = elements.FirstOrDefault(x => x.Designation == htmlElement.Designation);

                if (element == null)
                {
                    if (htmlElement.Parent != null) 
                    { 
                        var parentDesignation = htmlElement.Parent.Designation;
                        var excelParent = elements.FirstOrDefault(x => x.Designation == parentDesignation);
                        var ad = elements.Where(x => x.Designation == parentDesignation);
                        if (excelParent == null)
                            continue;
                        htmlElement.Parent = excelParent;
                        excelParent.Children.Add(htmlElement);
                    }
                    elements.Add(htmlElement);
                }
                else
                {
                    htmlElement.Pos = element.Pos;

                }
            }
        }

        private List<Element> GetDataFromExcel(InitialData initialData)
        {
            var excelPath = initialData.ExcelFile;

            if (!File.Exists(excelPath))
                return new List<Element>();

            var config = CreateExcelElementConfig(initialData, excelPath);

            return _excelElementParser.GetExcelElements(config);
        }


        private List<Element> GetDataFromHtml(InitialData initialData)
        {
            var elements = _htmlElementParcer.GetElementsFromHTML(initialData.HtmlFile);
            return elements;
        }

        private ExcelElementConfig CreateExcelElementConfig(InitialData initialData, string excelPath)
        {
            return new ExcelElementConfig
            {
                ExcelPath = excelPath,
                SheetNumber = initialData.SheetNumber,
                StartRow = initialData.StartRow,
                EndRow = initialData.EndRow,
                ProductIDCell = initialData.ProductIDCell,
                ProductNameCell = initialData.ProductNameCell,
                PositionColumn = initialData.PositionColumn,
                DesignationColumn = initialData.DesignationColumn,
                DescriptionColumn = initialData.DescriptionColumn,
                QuantityColumn = initialData.QuantityColumn,
                MakeOrBuyColumn = initialData.MakeBuyColumn,
                RevisionColumn = initialData.RevisionColumn,
                ItemCodeSupplier = initialData.ItemCodeSupplierColumn,
                Costtype = initialData.CosttypeColumn,
                Spare = initialData.SpareColumn,
                AddInfo = initialData.AddInfoColumn
            };
        }

        private void MakeAdditionalParamters(List<Element> htmlElements, List<Element> excelElements)
        {
            foreach (var element in htmlElements)
            {
                if (element.Designation.Contains("447020433"))
                {

                }
                if (element.Parent == null)
                {
                    element.AssemblyFile = _parametersDefinder.DefineRootElementAssembly(element);
                    continue;
                }
                var excelElement = excelElements.FirstOrDefault(x=>!x.Root && x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
                if (excelElement != null)
                {
                    element.Quantity = excelElement.Quantity;
                    element.Costtype = excelElement.Costtype;
                    element.MakeOrBuy = _parametersDefinder.DefineMakeBuy(excelElement.MakeOrBuy);
                    element.Spare = _parametersDefinder.DefineSpare(excelElement.Spare);
                    element.ItemCodeSupplier = excelElement.ItemCodeSupplier;
                    element.AddInfo = excelElement.AddInfo;
                }
                else
                {
                    element.Quantity = "1";
                }

               element.TreeType = _parametersDefinder.DefineElementType(element);
                _parametersDefinder.DefineFiles(element);
            }



        }

        private void MatchQuantity(List<Element> htmlElements, List<Element> excelElements)
        {
            foreach (var element in htmlElements)
            {
                if (element.Parent == null)
                    continue;


                var excelElement = excelElements.FirstOrDefault(x=>!x.Root && x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
                if (excelElement != null)
                    element.Quantity = excelElement.Quantity;
                else
                    element.Quantity = "1";
            }
        }

        private void FillFileNames(List<Element> collection, string baseDirectory)
        {
            foreach (var element in collection)
            {
                var designation = element.Designation;
                var foundPathFile = _fileSearchService.FindFilesWithCriteria(baseDirectory, designation, ".SLDDRW").FirstOrDefault();
                if (!string.IsNullOrEmpty(foundPathFile))
                {
                    var fileName = Path.GetFileName(foundPathFile);
                    element.DrawingFile = fileName;
                }

                var foundDetails = _fileSearchService.FindFilesWithCriteria(baseDirectory, designation, ".SLDPRT").FirstOrDefault();
                if (!string.IsNullOrEmpty(foundDetails))
                {
                    var fileName = Path.GetFileName(foundDetails);
                    element.PartFile = fileName;
                }
                var foundAssemblies = _fileSearchService.FindFilesWithCriteria(baseDirectory, designation, ".SLDASM").FirstOrDefault();
                if (!string.IsNullOrEmpty(foundAssemblies))
                {
                    var fileName = Path.GetFileName(foundAssemblies);
                    element.AssemblyFile = fileName;
                }
            }
        }

        private void LinkDocumentsToDetails(List<Element> elements)
        {
            // Создаем список для удаления
            var elementsToRemove = new List<Element>();

            foreach (var element in elements)
            {
                if (element.Designation.Contains("BORDER"))
                {
                    elementsToRemove.Add(element);
                }
                if (element.Children?.Count > 0)
                {
                    foreach (var child in element.Children)
                    {
                        if (child.DrawingIcon == ElementConstants.PDF)
                        {
                            element.DocFile = string.Format("{0}.{1}", child.Designation, "dox");
                            elementsToRemove.Add(child);
                        }
                        if (child.DrawingIcon == ElementConstants.DOC)
                        {
                            element.PDFFile = string.Format("{0}.{1}", child.Designation, "pdf");
                            elementsToRemove.Add(child);
                        }
                        if (child.DrawingIcon == ElementConstants.GIF)
                        {
                            if (element.Designation.Contains("BORDER") || child.Designation.Contains("BORDER"))
                            {

                            }
                            element.JpegFile = string.Format("{0}.{1}", child.Designation, "gif");
                            elementsToRemove.Add(child);
                        }
                        if (child.DrawingIcon == ElementConstants.GENERIC)
                        {
                            elementsToRemove.Add(child);
                        }
                    }
                }
            }

            foreach (var element in elementsToRemove)
            {
                elements.Remove(element);
            }
        }

        private void FindFiles(List<Element> elements, InitialData initialData)
        {
            var baseDirectory = initialData.BaseDirectory;
            foreach (var element in elements)
            {
                if (element.DrawingIcon == ElementConstants.DETAIL || element.DrawingIcon == ElementConstants.ASSEMBLY)
                {
                    var matchingFiles = _fileSearchService.FindFilesWithCriteria(baseDirectory, element.Designation, ".SLDDRW").ToList();
                    foreach (var matchingFile in matchingFiles)
                    {
                        var fileName = Path.GetFileName(matchingFile);
                        if (matchingFile.Contains("EA"))
                            element.EADrawingFile = fileName;
                       else if (matchingFile.Contains("RE"))
                            element.REDrawingFile = fileName;
                        else
                            element.EADrawingFile = fileName;
                    }
                    
                    //Так смотри, у каждого элемента я беру Designation и рекурсивно ищу во всех подпкаках BaseDirectory имена
                    //файлов, которые содержат designation и расширение SLDDRW.
                }
            }
        }
    }
}
