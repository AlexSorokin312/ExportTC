using ExportTC.Constants;
using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

public static class RootAssembly
{
   public static string value = string.Empty;
} 

namespace ExportTC.Model
{
    internal class AssemblyConstructor
    {
        public List<Element> savedElements = new();

        private readonly ExcelElementParser _excelElementParser;
        private readonly HtmlElementParser _htmlElementParcer;
        private readonly IFileSearchService _fileSearchService;
        private readonly IExcelReaderFactory _excelFactory;
        private readonly ParametersDefinder _parametersDefinder;
        private readonly RecordManager _recodeManager;

        public AssemblyConstructor(ExcelElementParser excelElementParser,
            HtmlElementParser htmlElementParcer,
            IExcelReaderFactory excelFactory,
            IFileSearchService fileSearchService,
            RecordManager recodeManager)
        {
            _htmlElementParcer = htmlElementParcer;
            _excelElementParser = excelElementParser;
            _excelFactory = excelFactory;
            _fileSearchService = fileSearchService;
            _recodeManager = recodeManager;
            _parametersDefinder = new ParametersDefinder();
        }

        string RootString = string.Empty;   

        public Assembly GetAssembly(InitialData initialData)
        {
            if (initialData == null)
            {
                LoggerDebug.LogError("InitialData передан как null.");
                throw new ArgumentNullException(nameof(initialData));
            }

            LoggerDebug.LogInfo("Получение данных из Excel.");
            var excelElements = GetDataFromExcel(initialData);

            LoggerDebug.LogInfo("Получение данных из HTML.");
            var htmlElements = GetDataFromHtml(initialData);

            //var m = htmlElements.FirstOrDefault();
            //m.Designation = "111111111";
            //m.Name = "111111111";

            LoggerDebug.LogInfo("Фильтрация элементов HTML на основе регулярного выражения.");
            RootAssembly.value = excelElements.FirstOrDefault().Designation;
            htmlElements = htmlElements
               .Where(e => !Regex.IsMatch(e.Designation, @"\b[А-ЯA-Z]\d+\b", RegexOptions.IgnoreCase))
               .ToList();

            RootString = htmlElements.FirstOrDefault().Designation;

            LoggerDebug.LogInfo("Слияние данных из Excel и HTML.");
            MergeExcelElementsWithHtmlData(excelElements, htmlElements);

            MatchQuantity(htmlElements, excelElements);

            LoggerDebug.LogInfo("Добавление дополнительных параметров.");
            MakeAdditionalParamters(htmlElements, excelElements);

            LoggerDebug.LogInfo("Заполнение имен файлов.");
            FillFileNames(htmlElements, initialData.BaseDirectory);

            LoggerDebug.LogInfo("Связывание документов с деталями.");
            LinkDocumentsToDetails(htmlElements);

            LoggerDebug.LogInfo("Поиск файлов для элементов.");
            FindFiles(htmlElements, initialData);

            var assembly = new Assembly(htmlElements);

            LoggerDebug.LogInfo("Сортировка элементов сборки.");
            assembly.Sort();

            LoggerDebug.LogInfo("Сборка успешно создана.");
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
                    htmlElement.Name = element.Name;
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
                var excelElement = excelElements.FirstOrDefault(x => !x.Root && x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
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

        //На случай если нужно будет читать данные о количестве из excel
        private void MatchQuantity(List<Element> htmlElements, List<Element> excelElements)
        {
            /*foreach (var element in htmlElements)
            {
                if (element.Parent == null)
                    continue;


                var excelElement = excelElements.FirstOrDefault(x => !x.Root && x.Designation == element.Designation && x.Parent.Designation == element.Parent.Designation);
                if (excelElement != null)
                {

                    if (element.Quantity != excelElement.Quantity) ;

                }
                else
                    element.Quantity = "1";
            }*/
        }

        private void FillFileNames(List<Element> collection, string baseDirectory)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Параллельная обработка коллекции
            Parallel.ForEach(collection, element =>
            {
                var designation = element.Designation;

                // Поиск деталей
                var foundDetails = _fileSearchService.FindFilesWithCriteria(baseDirectory, designation, ".SLDPRT").FirstOrDefault();
                if (!string.IsNullOrEmpty(foundDetails))
                {
                    var fileName = Path.GetFileName(foundDetails);
                    element.PartFile = fileName;
                }

                // Поиск сборок
                var foundAssemblies = _fileSearchService.FindFilesWithCriteria(baseDirectory, designation, ".SLDASM").FirstOrDefault();
                if (!string.IsNullOrEmpty(foundAssemblies))
                {
                    var fileName = Path.GetFileName(foundAssemblies);
                    element.AssemblyFile = fileName;
                }
            });

            watch.Stop();
            Console.WriteLine($"Метод выполнен за {watch.ElapsedMilliseconds} ms");
        }
        private void LinkDocumentsToDetails(List<Element> elements)
        {
            foreach (var element in elements)
            {
                if (element.Children?.Count > 0)
                {
                    // Элементы с пустым FileName оставляем как есть
                    var childrenWithoutFileName = element.Children
                        .Where(c => string.IsNullOrEmpty(c.FileName))
                        .ToList();

                    // Элементы с заполненным FileName группируем, чтобы оставить уникальные
                    var uniqueChildrenWithFileName = element.Children
                        .Where(c => !string.IsNullOrEmpty(c.FileName))
                        .GroupBy(c => c.FileName)
                        .Select(g => g.First())
                        .ToList();

                    // Объединяем обе коллекции: уникальные с FileName + те, у кого FileName пустой
                    element.Children = uniqueChildrenWithFileName.Concat(childrenWithoutFileName).ToList();
                    foreach (var uniqueChild in element.Children)
                    {
                        if (uniqueChild.DrawingIcon == null)
                        {
                            var previos = elements.FirstOrDefault(x=>x.Designation == uniqueChild.Designation);
                            uniqueChild.DrawingIcon = previos?.DrawingIcon;
                        }
                        HandleChild(element, uniqueChild);
                    }
                }
            }

            foreach (var element in FilesToRemoveFromCollection)
            {
                if (element.Designation == "448002293")
                {

                }
                var remove = FilesToRemoveFromCollection.Where(x => x.Designation == element.Designation).ToList();
                remove.ForEach(x=>elements.Remove(x));

            }

        }

        public List<Element> FilesToRemoveFromCollection = new();

        private void HandleChild(Element parent, Element child)
        {
            if (child.DrawingIcon == ElementConstants.DETAIL || child.DrawingIcon == ElementConstants.ASSEMBLY || child.DrawingIcon == ElementConstants.GENERIC)
                return;

            var exists = _recodeManager.IsRecordExists(RootString, parent.Designation, parent.Revision);
            if (exists)
            {
                if (child.Designation.Contains("448000164"))
                {

                }
                FilesToRemoveFromCollection.Add(child);
                return;
            }

            if (child.DrawingIcon == ElementConstants.PDF)
            {
                child.PDFFile = string.Format("{0}.{1}", child.Designation, "pdf");
                child.Designation = parent.Designation;
                child.Name = parent.Name;
                child.Parent = parent.Parent ?? parent;
                child.Revision = parent.Revision;
                CacheFileNames.fileNames.Add(child.FileName);
            }
            else if (child.DrawingIcon == ElementConstants.DOC)
            {
                if (child.FileName.Contains("docx"))
                {
                    child.DocxFile = string.Format("{0}.{1}", child.Designation, "docx");
                }
                else
                {
                    child.DocFile = string.Format("{0}.{1}", child.Designation, "doc");
                }

                child.Designation = parent.Designation;
                child.Name = parent.Name;
                child.Parent = parent.Parent ?? parent;
                child.Revision = parent.Revision;
                CacheFileNames.fileNames.Add(child.FileName);

            }
            /*else if (child.DrawingIcon == ElementConstants.GIF)
            {
                child.JpegFile = string.Format("{0}.{1}", child.Designation, "jpg");
                child.Designation = parent.Designation;
                child.Name = parent.Name;
                child.Parent = parent.Parent ?? parent;
                child.Revision = parent.Revision;
                CacheFileNames.fileNames.Add(child.FileName);
            }*/
            /* else if (child.DrawingIcon == ElementConstants.ZIP)
             {
                 child.ZipFile = string.Format("{0}.{1}", child.Designation, "zip");
                 child.Designation = parent.Designation;
                 child.Name = parent.Name;
                 child.Parent = parent.Parent ?? parent;
                 child.Revision = parent.Revision;
                 CacheFileNames.fileNames.Add(child.FileName);
             }*/
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(child.FileName))
                    {
                        if (child.FileName.Contains("xl", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.ExcelFile = string.Format("{0}.{1}", child.Designation, "xlsm");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("rar", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.ZipFile = string.Format("{0}.{1}", child.Designation, "rar");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("jpg", comparisonType: StringComparison.OrdinalIgnoreCase) || child.FileName.Contains("jpeg", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.JpegFile = string.Format("{0}.{1}", child.Designation, "jpg");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("htm", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.Html = string.Format("{0}.{1}", child.Designation, "htm");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("png", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.PNG = string.Format("{0}.{1}", child.Designation, "png");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("gif", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.GIF = string.Format("{0}.{1}", child.Designation, "gif");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("tif", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.TIF = string.Format(child.FileName);
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("dwg", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.DWG = string.Format("{0}.{1}", child.Designation, "dwg");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("dfx", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.DFX = string.Format("{0}.{1}", child.Designation, "dfx");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("dfx", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.DFX = string.Format("{0}.{1}", child.Designation, "dfx");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("zip", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.ZipFile = string.Format("{0}.{1}", child.Designation, "zip");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("step", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.STEP = string.Format("{0}.{1}", child.Designation, "step");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("ppt", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.PPT = string.Format("{0}.{1}", child.Designation, "ppt");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("pptx", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.PPTX = string.Format("{0}.{1}", child.Designation, "pptx");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("txt", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.TXT = string.Format("{0}.{1}", child.Designation, "txt");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("bmp", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.BMP = string.Format("{0}.{1}", child.Designation, "bmp");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else if (child.FileName.Contains("msg", comparisonType: StringComparison.OrdinalIgnoreCase))
                        {
                            child.MSG = string.Format("{0}.{1}", child.Designation, "msg");
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                        else
                        {
                            child.Multy = string.Format(child.FileName);
                            child.Designation = parent.Designation;
                            child.Name = parent.Name;
                            child.Parent = parent.Parent ?? parent;
                            child.Revision = parent.Revision;
                            CacheFileNames.fileNames.Add(child.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                
                }
            }
        }

        private void FindFiles(List<Element> elements, InitialData initialData)
        {
            var baseDirectory = initialData.BaseDirectory;

            // Параллельная обработка элементов
            Parallel.ForEach(elements, element =>
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
                        else if (matchingFile.Contains("EM"))
                            element.EMDrawingFile = fileName;
                        else
                            element.DrawingFile = fileName;
                    }
                }
            });
        }
    }
}
