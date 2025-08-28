using ExportTC.Constants;
using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Text.RegularExpressions;

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

            LoggerDebug.LogInfo("Фильтрация элементов HTML на основе регулярного выражения.");
            RootAssembly.value = excelElements.FirstOrDefault().Designation;

            var root = htmlElements.FirstOrDefault();
            var detailsINroot = root.Children.Where(x => x.DrawingIcon == ElementConstants.DETAIL).ToList();
            foreach (var detail in detailsINroot)
            {
                root.Children.Remove(detail);
            }

            htmlElements = htmlElements
               .Where(e => !Regex.IsMatch(e.Designation, @"\b[А-ЯA-Z]\d+\b", RegexOptions.IgnoreCase))
               .ToList();

            htmlElements = RemoveDuplicatesByDesignationAndParentDesignation(htmlElements);
            RootString = htmlElements.FirstOrDefault().Designation;

            LoggerDebug.LogInfo("Слияние данных из Excel и HTML.");

            LoggerDebug.LogInfo("Добавление дополнительных параметров.");
            MergeExcelElementsWithHtmlData(excelElements, htmlElements);

            MakeAdditionalParamters(htmlElements, excelElements);
            LoggerDebug.LogInfo("Заполнение имен файлов.");
            FillFileNames(htmlElements, initialData.BaseDirectory);

            LoggerDebug.LogInfo("Связывание документов с деталями.");

            LinkDocumentsToDetails(htmlElements);

            LoggerDebug.LogInfo("Поиск файлов для элементов.");
            FindFiles(htmlElements, initialData);
            MatchQuantity1(htmlElements);

            SingleGenerics(htmlElements);

            RemoveDrawingDuplicates(htmlElements);

            var assembly = new Assembly(htmlElements);

            LoggerDebug.LogInfo("Сортировка элементов сборки.");
            assembly.Sort();

            LoggerDebug.LogInfo("Сборка успешно создана.");
            return assembly;
        }

        public static List<Element> RemoveDuplicatesByDesignationAndParentDesignation(List<Element> elements)
        {
            if (elements == null || elements.Count == 0)
            {
                return new List<Element>(); // Возвращаем пустой список, если входной список пуст или null
            }

            // Группируем элементы по комбинации Designation и Parent.Designation
            var uniqueElements = elements
                .Where(e => e != null && e.Designation != null && e.Parent != null && e.Parent.Designation != null) // Убираем null значения
                .GroupBy(e => new { e.Designation, ParentDesignation = e.Parent.Designation }) // Группировка по Designation и Parent.Designation
                .Select(group => group.First()) // Выбираем первый элемент из каждой группы
                .ToList();
            return uniqueElements;
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

                    htmlElement.Costtype = element.Costtype;
                    htmlElement.MakeOrBuy = element.MakeOrBuy?.ToUpper();
                    htmlElement.Spare = element.Spare;
                    htmlElement.ItemCodeSupplier = element.ItemCodeSupplier;
                    htmlElement.AddInfo = element.AddInfo;
                    htmlElement.HenconStatus = element.HenconStatus;
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
                AddInfo = initialData.AddInfoColumn,
                HenconStd = initialData.HenconStdColumn
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

                    element.Costtype = excelElement.Costtype;
                    element.MakeOrBuy = excelElement.MakeOrBuy;
                    element.Spare = excelElement.Spare;
                    element.ItemCodeSupplier = excelElement.ItemCodeSupplier;
                    element.AddInfo = excelElement.AddInfo;
                    element.HenconStatus = excelElement.HenconStatus;

                }

                element.TreeType = _parametersDefinder.DefineElementType(element);
                _parametersDefinder.DefineFiles(element);
            }
        }

        private Dictionary<string, string> _numberes = new();
        List<string> quantity = new List<string>();

        ////На случай если нужно будет читать данные о количестве из excel
        private void MatchQuantity1(List<Element> htmlElements)
        {
            var generics = htmlElements.Where(x => x.DrawingIcon == ElementConstants.GENERIC || x.DrawingIcon == ElementConstants.BOM);
            string row = string.Empty;
            foreach (var generi in generics)
            {
                row = String.Format("{0}-{1}", generi.Parent.Designation, generi.Designation);

                if (!_numberes.ContainsKey(row))
                {

                    _numberes.Add(row, generi.Quantity);

                }
            }

            htmlElements.Skip(1).ToList().ForEach(x => x.Quantity = "0");
            htmlElements.FirstOrDefault().Children.ForEach(x => x.Quantity = "1");

            //Проверка по кешу всех выгрзок
            foreach (var element in htmlElements.Skip(1))
            {
                try
                {
                    var exists = _recodeManager.IsRecordExists(RootString, element.Parent.Designation, element.Parent.Revision);
                    if (exists)
                    {
                        if (_recodeManager.IsRecordExists(RootString, element.Designation, element.Revision))
                        {
                            element.Quantity = "0";

                        }
                        else
                        {
                            element.Quantity = "1";
                            if (element.DrawingIcon == ElementConstants.GENERIC)
                            {
                                row = String.Format("{0}-{1}", element.Parent.Designation, element.Designation);
                                var first = _numberes.FirstOrDefault(x => x.Key == row);
                                element.Quantity = first.Value;
                            }
                        }
                    }
                    else
                    {
                        element.Quantity = "1";
                        if (element.DrawingIcon == ElementConstants.GENERIC)
                        {
                            row = String.Format("{0}-{1}", element.Parent.Designation, element.Designation);
                            var first = _numberes.FirstOrDefault(x => x.Key == row);
                            element.Quantity = first.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            //Проверка по внутренним сборкам
            var assemblies = htmlElements.Where(x => x.DrawingIcon == ElementConstants.ASSEMBLY);
            var assembBydesignation = assemblies.GroupBy(x => x.Designation);
        }


        /// <summary>
        /// Рекурсивно обнуляет Quantity у всех дочерних элементов (и их потомков).
        /// </summary>
        private void ZeroQuantityRecursively(Element parent)
        {
            // Пробегаем по всем детским элементам
            foreach (var child in parent.Children)
            {

                // Обнуляем
                child.Quantity = "0";

                // Аналогичным образом обходим детей этого child
                if (child.Children?.Count > 0)
                {
                    ZeroQuantityRecursively(child);
                }
            }
        }


        private void FillFileNames(List<Element> collection, string baseDirectory)
        {
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
        }

        List<Element> newElements = new();
        List<Element> toRemove = new();

        private void LinkDocumentsToDetails(List<Element> elements)
        {
            // Отфильтруем элементы, которые не являются файлами или сборками
            var noFilesAndAssemblies = elements
                .Where(x => x.DrawingIcon != ElementConstants.ASSEMBLY && x.DrawingIcon != ElementConstants.DETAIL)
                .ToList();

            foreach (var file in noFilesAndAssemblies)
            {

                var parents = elements
                      .Where(parent => parent.Children?.Any(child => child.Designation == file.Designation
                      &&  child.DrawingIcon != ElementConstants.DETAIL
                      && child.DrawingIcon != ElementConstants.ASSEMBLY) ?? false)
                      .ToList();

                file.Parents = parents;

                HandleElement(file);
            }

            toRemove.ForEach(x => elements.Remove(x));
            elements.AddRange(newElements);

            var m = elements.Where(x => x.Multy != null);
            var m1 = elements.Where(x => x.Multy != null);
            toRemove.Clear();
            newElements.Clear();
        }

        private string fileNameSave = "Skipped.txt";

        private void HandleElement(Element child)
        {
            if (string.IsNullOrEmpty(child.FileName))
                return;

            var fileName = child.FileName;
            if (fileName.Contains(".bom", StringComparison.OrdinalIgnoreCase))
                return;

            var parents = child.Parents;

            foreach (var parent in parents)
            {
                if (parent.DrawingIcon != ElementConstants.ASSEMBLY && parent.DrawingIcon != ElementConstants.DETAIL && parent.DrawingIcon != ElementConstants.GENERIC)
                {
                    var fileName1 = parent.FileName;
                    toRemove.Add(child);
                    SetFileName(fileName1, parent);
                    WriteLog(child);
                    parent.Children?.Remove(child);
                    parent.Designation = parent.Parent.Designation;
                    parent.Parent.Parent.Children.Add(parent);
                    continue;
                }
                var neededChild = parent.Children != null
                    ? parent.Children.FirstOrDefault(x => x.Designation != null && x.Designation.Contains(child.Designation) && x.DrawingIcon != ElementConstants.DETAIL && x.DrawingIcon != ElementConstants.ASSEMBLY)
                    : null;
                toRemove.Add(neededChild);
                if (neededChild != null)
                    parent.Children?.Remove(neededChild);

                var newChild = new Element();
                newChild.Designation = parent.Designation;
                newChild.Name = parent.Name;
                newChild.Parent = parent.Parent;
                newChild.TreeType = parent.TreeType;

                newChild.FileName = child.FileName;
                newChild.Revision = parent.Revision;
                CacheFileNames.fileNames.Add(child.FileName);
                parent.Parent.Children.Add(newChild);
                newChild.Parents?.Add(parent);

                if (!newChild.Parent.Designation.Contains("General"))
                    newElements.Add(newChild);
                else
                {
                    parent.Children?.Remove(neededChild);
                    toRemove.Add(child);
                }

                if (fileName.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                    newChild.PDFFile = fileName;

                else if (fileName.Contains("xl", StringComparison.OrdinalIgnoreCase))
                    newChild.ExcelFile = fileName;

                else if (fileName.Contains("zip", StringComparison.OrdinalIgnoreCase))
                    newChild.ZipFile = fileName;

                else if (fileName.Contains("rar", StringComparison.OrdinalIgnoreCase))
                    newChild.ZipFile = fileName;

                else if (fileName.Contains("jpg", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.JpegFile = fileName;

                else if (fileName.Contains("png", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.PNG = fileName;

                else if (fileName.Contains("htm", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.Html = fileName;

                else if (fileName.Contains("7z", StringComparison.OrdinalIgnoreCase))
                    child.ZipFile = fileName;

                else if (fileName.Contains("gif", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.GIF = fileName;

                else if (fileName.Contains("dxf", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.DXF = fileName;

                else if (fileName.Contains("zip", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.ZipFile = fileName;

                else if (fileName.Contains("stp", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.STEP = fileName;

                else if (fileName.Contains("ppt", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.PPT = fileName;

                else if (fileName.Contains("ppt", comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    if (fileName.Contains("pptx", comparisonType: StringComparison.OrdinalIgnoreCase))
                    {
                        newChild.PPTX = fileName;
                    }
                    else
                    {
                        newChild.PPT = fileName;

                    }
                }

                else if (fileName.Contains("tif", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.TIF = fileName;

                else if (fileName.Contains("txt", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.TXT = fileName;

                else if (fileName.Contains("bmp", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.BMP = fileName;

                else if (fileName.Contains("msg", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.MSG = fileName;

                else if (fileName.Contains("dwg", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.DWG = fileName;

                else if (fileName.Contains(".E60", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.Multy = fileName;

                else if (fileName.Contains(".ccd", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.Multy = fileName;

                else if (fileName.Contains(".#PA", comparisonType: StringComparison.OrdinalIgnoreCase))
                    newChild.Multy = fileName;


                else if (fileName.Contains("doc"))
                {
                    if (newChild.FileName.Contains("docx"))
                        newChild.DocxFile = fileName;
                    else
                        newChild.DocFile = fileName;
                }
                else
                {
                    child.PartFile = null;
                    child.DrawingFile = null;
                    child.Quantity = "0";
                    child.Multy = string.Format(child.FileName);
                    child.Designation = parent.Designation;
                    child.Name = parent.Name;
                    child.Parent = parent.Parent ?? parent;
                    child.Revision = parent.Revision;
                    CacheFileNames.fileNames.Add(child.FileName);
                }
            }
        }
        private void WriteLog(Element child)
        {
            // Формируем строку, которую хотим добавить
            string lineToAdd = $"{child.Designation} не был добавлен. Родительский элемент {child.Parent.Designation}\n";

            // Считываем все строки из файла (если файл существует)
            string[] lines = File.Exists(fileNameSave) ? File.ReadAllLines(fileNameSave) : new string[0];

            // Проверяем, есть ли уже такая строка
            if (!lines.Contains(lineToAdd))
            {
                // Если нет, добавляем строку с переносом строки
                File.AppendAllText(fileNameSave, lineToAdd + Environment.NewLine);
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

                        if (matchingFile.Contains("EA", StringComparison.OrdinalIgnoreCase))
                            element.EADrawingFile += fileName + ", ";
                        else if (matchingFile.Contains("RE"))
                            element.REDrawingFile += fileName + ", ";
                        else if (matchingFile.Contains("EM", StringComparison.OrdinalIgnoreCase))
                        {
                            element.EMDrawingFile += fileName + ", ";
                        }
                        else
                            element.DrawingFile += fileName + ", ";
                    }
                }
            });
        }

        public void RemoveDrawingDuplicates(List<Element> elements)
        {
            var details = elements
                .Where(x => x.DrawingIcon == ElementConstants.DETAIL || x.DrawingIcon == ElementConstants.ASSEMBLY)
                .ToList();

            // Вызываем метод для каждого нужного поля:
            RemoveDuplicatesForField(
           details,
              e => e.PartFile,
             (e, val) => e.PartFile = val
            );

            RemoveDuplicatesForField(
                details,
                e => e.DrawingFile,
                (e, val) => e.DrawingFile = val
            );

            RemoveDuplicatesForField(
                details,
                e => e.EADrawingFile,
                (e, val) => e.EADrawingFile = val
            );

            RemoveDuplicatesForField(
                details,
                e => e.REDrawingFile,
                (e, val) => e.REDrawingFile = val
            );

            RemoveDuplicatesForField(
                details,
                e => e.EMDrawingFile,
                (e, val) => e.EMDrawingFile = val
            );

            RemoveDuplicatesForField(
                details,
                e => e.AssemblyFile,
                (e, val) => e.AssemblyFile = val);
        }

        /// <summary>
        /// Универсальный метод для "обнуления" поля у повторных элементов при группировке.
        /// </summary>
        /// <param name="elements">Список элементов</param>
        /// <param name="propertyGetter">Функция, получающая нужное строковое поле</param>
        /// <param name="propertySetter">Функция, устанавливающая значение этого поля</param>
        private void RemoveDuplicatesForField(
            List<Element> elements,
            Func<Element, string?> propertyGetter,
            Action<Element, string?> propertySetter)
        {
            var groups = elements.GroupBy(propertyGetter);

            foreach (var group in groups)
            {
                bool isFirst = true;
                foreach (var elem in group)
                {
                    if (isFirst)
                    {
                        // У первого элемента оставляем значение как есть
                        isFirst = false;
                    }
                    else
                    {
                        // У повторных – обнуляем
                        propertySetter(elem, null);
                    }
                }
            }
        }

        public void SingleGenerics(List<Element> elements)
        {
            var groups = elements.GroupBy(e => new { e.Designation, ParentDesignation = e.Parent.Designation });  //Группируем элементы по их имени родителя и имени

            foreach (var generic in groups)
            {
                if (generic.Count() == 2) //Находим коллекцию из двух элементов - это значит, что это один родительский объект, в который входит один дочерний
                {
                    var noGeneric = generic.FirstOrDefault();
                    var noDetail = generic.LastOrDefault();

                    var parentEqual = noGeneric.Parents.Where(x=>x.Designation == noGeneric.Designation); //Случай, когда один из бомов совпадает по обозначению с родителем и этот бом не единственный
                    if (parentEqual.Any())
                    {
                        return;
                    }   

                    noGeneric.PNG = noDetail.PNG;
                    noGeneric.PDFFile = noDetail.PDFFile;
                    noGeneric.ExcelFile = noDetail.ExcelFile;
                    noGeneric.ZipFile = noDetail.ZipFile;
                    noGeneric.TIF = noDetail.TIF;
                    noGeneric.JpegFile = noDetail.JpegFile;
                    noGeneric.Html = noDetail.Html;
                    noGeneric.DocFile = noDetail.DocFile;
                    noGeneric.DocxFile = noDetail.DocxFile;
                    noGeneric.MSG = noDetail.MSG;
                    noGeneric.BMP = noDetail.BMP;
                    noGeneric.STEP = noDetail.STEP;
                    noGeneric.DXF = noDetail.DXF;
                    noGeneric.DWG = noDetail.DWG;
                    noGeneric.PNG = noDetail.PNG;
                    noGeneric.TXT = noDetail.TXT;
                    noGeneric.GIF = noDetail.GIF;
                    noGeneric.Multy = noDetail.Multy;
                    elements.Remove(noDetail);
                }
            }
        }


        private void ProcessSingleFile(
            List<Element> elements,
            Func<Element, string> getFile,
            Action<Element, string> setFile)
        {
            // Выбираем элементы с нужным DrawingIcon
            var generics = elements
                .Where(x => x.DrawingIcon == ElementConstants.GENERIC ||
                            x.DrawingIcon == ElementConstants.BOM ||
                            x.DrawingIcon == ElementConstants.DETAIL ||
                            x.DrawingIcon == ElementConstants.ASSEMBLY)
                .ToList();

            // Хранить уже обработанные файлы с учётом родительского элемента
            var existingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var element in generics)
            {
                // Дополнительная логика для конкретного Designation, если требуется

                foreach (var child in element.Children)
                {
                    var fileValue = getFile(child);

                    // Если значение файла не пустое
                    if (!string.IsNullOrEmpty(fileValue))
                    {
                        // Формируем составной ключ, объединяя идентификатор родительского элемента и имя файла
                        var compositeKey = $"{child.Designation}{element.Designation}-{fileValue}";
                        if (!existingFiles.Contains(compositeKey))
                        {
                            existingFiles.Add(compositeKey);
                        }
                        else
                        {
                            elements.Remove(child);
                        }
                    }
                }
            }
        }




        private void SetFileName(string fileName, Element child)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (fileName.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                child.PDFFile = fileName;

            else if (fileName.Contains("xl", StringComparison.OrdinalIgnoreCase))
                child.ExcelFile = fileName;

            else if (fileName.Contains("zip", StringComparison.OrdinalIgnoreCase))
                child.ZipFile = fileName;

            else if (fileName.Contains("rar", StringComparison.OrdinalIgnoreCase))
                child.ZipFile = fileName;

            else if (fileName.Contains("7z", StringComparison.OrdinalIgnoreCase))
                child.ZipFile = fileName;

            else if (fileName.Contains("jpg", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.JpegFile = fileName;

            else if (fileName.Contains("png", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.PNG = fileName;

            else if (fileName.Contains("htm", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.Html = fileName;

            else if (fileName.Contains("gif", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.GIF = fileName;

            else if (fileName.Contains("dxf", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.DXF = fileName;

            else if (fileName.Contains("zip", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.ZipFile = fileName;

            else if (fileName.Contains("stp", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.STEP = fileName;

            else if (fileName.Contains("ppt", comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.Contains("pptx", comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    child.PPTX = fileName;
                }
                else
                {
                    child.PPT = fileName;

                }
            }

            else if (fileName.Contains("tif", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.TIF = fileName;

            else if (fileName.Contains("txt", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.TXT = fileName;

            else if (fileName.Contains("bmp", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.BMP = fileName;

            else if (fileName.Contains("msg", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.MSG = fileName;

            else if (fileName.Contains("dwg", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.DWG = fileName;

            else if (fileName.Contains(".E60", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.Multy = fileName;

            else if (fileName.Contains(".ccd", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.Multy = fileName;

            else if (fileName.Contains(".#PA", comparisonType: StringComparison.OrdinalIgnoreCase))
                child.Multy = fileName;



            else if (fileName.Contains("doc"))
            {
                if (child.FileName.Contains("docx"))
                    child.DocxFile = fileName;
                else
                    child.DocFile = fileName;
            }
        }
    }
}
