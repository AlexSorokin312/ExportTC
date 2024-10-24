using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Xml.Linq;

namespace ExportTC.Model
{
    internal class AssemblyConstructor
    {
        private readonly ExcelElementParser _excelElementParser;
        private readonly HtmlElementParser _htmlElementParcer;
        private readonly IFileSearchService _fileSearchService;
        private readonly IExcelReaderFactory _excelFactory;

        public AssemblyConstructor(ExcelElementParser excelElementParser,
            HtmlElementParser htmlElementParcer,
            IExcelReaderFactory excelFactory,
            IFileSearchService fileSearchService)
        {
            _htmlElementParcer = htmlElementParcer;
            _excelElementParser = excelElementParser;
            _excelFactory = excelFactory;
            _fileSearchService = fileSearchService;
        }

        public Assembly GetAssembly(InitialData initialData)
        {
            if (initialData == null)
                throw new ArgumentNullException(nameof(initialData));

            var excelElements = GetDataFromExcel(initialData);
            var htmlElements = GetDataFromHtml(initialData);
            MergeExcelElementsWithHtmlData(excelElements, htmlElements);
            MatchQuantity(htmlElements, excelElements);
            MakeAdditionalParamters(htmlElements, excelElements);
            FillFileNames(htmlElements, initialData.BaseDirectory);

            var assembly = new Assembly(htmlElements);
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
                    var parentDesignation = htmlElement.Parent.Designation;
                    var excelParent = elements.FirstOrDefault(x => x.Designation == parentDesignation);
                    htmlElement.Parent = excelParent;
                    excelParent.Children.Add(htmlElement);

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
                if (element.Parent == null)
                    continue;
                var excelElement = excelElements.FirstOrDefault(x => x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
                if (excelElement != null)
                {
                    element.Quantity = excelElement.Quantity;
                    element.Costtype = excelElement.Costtype;
                    element.MakeOrBuy = excelElement.MakeOrBuy;
                    element.Spare = excelElement.Spare;;
                    element.ItemCodeSupplier = excelElement.ItemCodeSupplier;
                    element.AddInfo = excelElement.AddInfo;
                }
                else
                    element.Quantity = "1";
            }
        }

        private void MatchQuantity(List<Element> htmlElements, List<Element> excelElements)
        {
            foreach (var element in htmlElements)
            {
                if (element.Parent == null)
                    continue;
                var excelElement = excelElements.FirstOrDefault(x=>x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
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
                    element.FileName = fileName;
                }
            }
        }
    }
}
