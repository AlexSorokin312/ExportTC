using ExportTC.Constants;
using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using System.IO;

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
                {
                    element.AssemblyFile = _parametersDefinder.DefineRootElementAssembly(element);
                    continue;
                }
                var excelElement = excelElements.FirstOrDefault(x => x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
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
                    element.DrawingFile = fileName;
                }
            }
        }
    }

    public class ParametersDefinder
    {
        public string DefineElementType(Element element)
        {
            if (element.DrawingIcon == ElementConstants.DETAIL)
                return ElementConstants.DETAIL;
            if (element.DrawingIcon == ElementConstants.ASSEMBLY)
                return ElementConstants.ASSEMBLY;

            return ElementConstants.DRAFT;
        }

        public string DefineRootElementAssembly(Element element)
        {
            return string.Format("{0}.{1}", element.Designation, "SLDASM");
        }

        public string DefineSpare(string spare)
        {
            if (spare == null)
                return string.Empty;

            spare = spare.Replace(';', ' ');

            if (spare.Contains("N/A"))
                return string.Empty;
            if (spare.Contains("M M"))
                return "M";

            return spare;
        }

        public string DefineMakeBuy(string makeBuy)
        {
            makeBuy = CommonConstants.GetMakeBuyReplacmentText(makeBuy);
            return makeBuy;
        }

        public void DefineFiles(Element element)
        {
            var fileName = element.FileName;
            if (fileName == null)
            {
                element.AssemblyFile  = element.Designation + ".SLDASM";
                return;
            }
    

            if (fileName.Contains(".pdf") || fileName.Contains(".PDF"))
                element.PDFFile = fileName;
                element.TCType = "PDF";


            if (fileName.Contains(".zip") || fileName.Contains(".ZIP"))
                element.ZipFile = fileName;
                element.TCType = "ZIP";

            if (fileName.Contains("SLDPRT") || fileName.Contains("sldprt"))
                element.PartFile = fileName;

            if (fileName.Contains("dwg") || fileName.Contains("DWG"))
                element.DrawingFile = fileName;

            if (fileName.Contains("doc") || fileName.Contains("DOC"))
            {
                if (fileName.Contains("docx") || fileName.Contains("DOCX"))
                    element.DocxFile = fileName;
                else
                    element.DocFile = fileName;
            }

            if (fileName.Contains("jpg") || fileName.Contains("JPG"))
                element.JpegFile = fileName;

        }
    }
}
