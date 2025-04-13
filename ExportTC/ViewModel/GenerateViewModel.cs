using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExportTC.Constants;
using ExportTC.Model;
using HenconExport;
using HenconExport.Model.Elemnts;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ExportTC.ViewModel
{
    internal class GenerateViewModel : ObservableObject
    {
        private InitialData _initialData;
        private RecordManager _recordManager;
        private IFileSearchService _fileSearchService;
        private Assembly _assembly;
        public ObservableCollection<Element> RootElements { get; private set; } = new ObservableCollection<Element>();
        private readonly RecordManager _recodeManager;
        private ObservableCollection<string> _comboBoxItems;
        public ObservableCollection<string> ComboBoxItems
        {
            get => _comboBoxItems;
            set => SetProperty(ref _comboBoxItems, value);
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }


        private string _selectedComboBoxItem;
        public string SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set => SetProperty(ref _selectedComboBoxItem, value);
        }
        public ICommand StartProcessCommand { get; }
        public ICommand GenerateCommand { get; }

        public GenerateViewModel()
        {
            try
            {
                _initialData = App.ServiceProvider.GetService<InitialData>()
                               ?? throw new InvalidOperationException(ErrorMessages.InitialDataServiceError);

                _fileSearchService = App.ServiceProvider.GetService<IFileSearchService>()
                                   ?? throw new InvalidOperationException(ErrorMessages.FileSearchServiceError);

                if (RootElements.Count != 0)
                    return;
                StartProcessCommand = new AsyncRelayCommand(SaveToExcelFileAsync);
                GenerateCommand = new AsyncRelayCommand(DisplayTree);
                _recordManager = App.ServiceProvider.GetService<RecordManager>()
                    ?? throw new InvalidOperationException(ErrorMessages.InitialDataServiceError);
                AppLogger.LogInformation("GenerateViewModel initialized successfully.");
            }
            catch (Exception ex)
            {
                AppLogger.LogFatal(ex, ErrorMessages.ViewModelInitializationError);
                throw;
            }
        }

        private void FindGenericFiles()
        {
            var path = _initialData.GenericFilePath; // Исходная директория поиска
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                LoggerDebug.LogWarning("Не найдена директория Generic-файлов");
                return;
            }

            string directoryToSave = Path.Combine(_initialData.BaseDirectory, "Hencon_Imp");// Задайте путь к директории для сохранения
            var cacheFileNames = CacheFileNames.fileNames;

            if (!Directory.Exists(directoryToSave))
            {
                Directory.CreateDirectory(directoryToSave);
            }

            Parallel.ForEach(cacheFileNames, fileName =>
            {
                var alreadyExists = Directory.EnumerateFiles(directoryToSave, "*", SearchOption.AllDirectories)
                             .Where(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));
                if (alreadyExists.Any())
                {

                }
                else
                {
                    var matchingFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                                                 .Where(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));

                    foreach (var file in matchingFiles)
                    {
                        string destinationPath = Path.Combine(directoryToSave, Path.GetFileName(file));

                        try
                        {
                            File.Copy(file, destinationPath, overwrite: true);
                            Console.WriteLine($"Файл {file} успешно скопирован в {destinationPath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при копировании файла {file}: {ex.Message}");
                        }
                    }
                }
            });
        }



        public async Task DisplayTree()
        {
            if (RootElements.Count > 0)
                return;
            try
            {
                var assemblyFiller = App.ServiceProvider.GetService<AssemblyConstructor>()
                                   ?? throw new InvalidOperationException(ErrorMessages.ViewModelInitializationError);

                _assembly = assemblyFiller.GetAssembly(_initialData);
                var rootElement = _assembly.GetRootElement();

                RootElements.Clear();
                //foreach (var element in rootElements)
                //{
                RootElements.Add(rootElement);
                //}
                FindGenericFiles();
                AppLogger.LogInformation("Tree successfully displayed.");
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.GenerateViewModelInitializationError);
                MessageBox.Show("An error occurred while displaying the tree. Check logs for details.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SaveToExcelFileAsync()
        {
            if (_assembly == null)
            {
                MessageBox.Show("Сначала нажмите кнопку \"Сформировать структуру\" для получения данных", "Предупреждение", MessageBoxButton.OK);
                return;
            }

            IsProgressVisible = true;
            ProgressValue = 0;

            string savePath = _initialData.SavePath;
            if (File.Exists(Path.Combine(savePath, "Hencon_Impl.xlsm")))
            try
            {
                File.Delete(savePath);
            }
            catch
            {
            }
            string outputPath = Path.Combine(savePath, CommonConstants.DefaultResultFileName);
            try
            {
                using (var resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Hencon_Impl.xlsm")).Stream)
                {
                    using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await resourceStream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (IOException ex)
            {
                AppLogger.LogError(ex, ErrorMessages.ErrorExcelFIleIsBusy);
                ShowErrorMessage("Файл Excel уже открыт. Пожалуйста, закройте его и попробуйте снова.");
                return;
            }

            try
            {
                using (var excelWriter = new ExcelWriter(outputPath))
                {
                    var worksheet = excelWriter.GetWorksheet(2);
                    int row = 3;
                    var elements = _assembly.Elements;
                    var result = CheckIncorrectFiles(elements.ToList());
                    if (!result)
                        return;

                    //_assembly.Sort(elements);
                    int totalElements = elements.Count;

                    var sortedElements = elements
                         .OrderBy(e => e.GetHierarchyDepth())
                         .ToList();

                    sortedElements = Sort(sortedElements.ToList());


                    for (int i = 0; i < totalElements; i++)
                    {
                        if (sortedElements[i].Parent != null && sortedElements[i].Parent.Designation.Contains("General"))
                        {
                            continue;
                        }
                        WriteElementToExcel(excelWriter, worksheet, row, sortedElements[i]);

                        row++;

                        // Обновляем прогресс
                        ProgressValue = (i + 1) * 100.0 / totalElements;
                    }
                    excelWriter.Save();
                }
            }
            catch (IOException ex)
            {
                // Обработка ошибки, если файл занят другим процессом (например, открыт в Excel)
                AppLogger.LogError(ex, "Ошибка при записи в файл Excel. Возможно, файл занят другим процессом.");
                ShowErrorMessage("Файл Excel уже открыт. Пожалуйста, закройте его и попробуйте снова.");
                return;
            }

            Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
        }


        private List<Element> Sort(List<Element> elements)
        {
            var seen = new HashSet<string>();
            var swappedChildren = new List<Element>();

            for (int i = 0; i < elements.Count; i++)
            {
                var child = elements[i];
                var parentDesignation = child.Parent?.Designation;
                if (!string.IsNullOrWhiteSpace(parentDesignation))
                {
                    int parentIndex = elements.FindIndex(e => e.Designation == parentDesignation);
                    if (parentIndex > i)
                    {
                        swappedChildren.Add(child);
                        (elements[i], elements[parentIndex]) = (elements[parentIndex], elements[i]);
                        i--; // re‑check current position, т.к. теперь тут родитель
                    }
                }
            }
            return elements;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private List<string> designationsWithFiles = new List<string>();

        private void WriteElementToExcel(ExcelWriter excelWriter, ExcelWorksheet worksheet, int row, Element element)
        {

            if (element.Parent == null)
                element.Quantity = "1";

            if (element.Parent != null)
            {
                if (element.Parent.Designation.Contains("General"))
                    return;
            }

            var existsFile = _recordManager.IsRecordExists(RootAssembly.value, element.Parent?.Designation, element.Parent?.Revision);


            if (element.Parent != null)
                excelWriter.WriteCell(worksheet, row, 1, element.Parent.Designation);

            excelWriter.WriteCell(worksheet, row, 2, "Элемент");

            excelWriter.WriteCell(worksheet, row, 3, element.Designation);
            excelWriter.WriteCell(worksheet, row, 4, element.Designation);
            excelWriter.WriteCell(worksheet, row, 5, element.Quantity);
            if (!string.IsNullOrEmpty(element.Name))
            {
                excelWriter.WriteCell(worksheet, row, 6, element.Name);
            }
            else
            {
                excelWriter.WriteCell(worksheet, row, 6, element.Designation);
            }
            excelWriter.WriteCell(worksheet, row, 10, element.Revision);
            excelWriter.WriteCell(worksheet, row, 14, element.Designation);

            if (!string.IsNullOrEmpty(element.Costtype))
            {
                // Приводим строку так, чтобы первая буква была заглавной, а остальные — маленькими.
                string formattedCosttype = char.ToUpper(element.Costtype[0]) + element.Costtype.Substring(1).ToLower();
                excelWriter.WriteCell(worksheet, row, 34, formattedCosttype);
            }

            excelWriter.WriteCell(worksheet, row, 35, element.MakeOrBuy?.ToUpper());

            if (!string.IsNullOrEmpty(element.HenconStatus))
            {
                string output = char.ToUpper(element.HenconStatus[0]) + element.HenconStatus.Substring(1).ToLower();
                if (output.Contains("hencon", StringComparison.OrdinalIgnoreCase))
                {
                    output = output.Replace("hencon", "Hencon");
                }
                excelWriter.WriteCell(worksheet, row, 74, output);
            }

            excelWriter.WriteCell(worksheet, row, 36, element.Spare?.ToUpper());
            excelWriter.WriteCell(worksheet, row, 37, element.ItemCodeSupplier);
            excelWriter.WriteCell(worksheet, row, 38, element.TreeType);
            if (element.TreeType == "Detail")
            {
                excelWriter.WriteCell(worksheet, row, 38, "Part");
            }

            excelWriter.WriteCell(worksheet, row, 15, element.Designation + "-" + element.Revision);
            if (element.Parent != null)
                excelWriter.WriteCell(worksheet, row, 16, string.Format("{0}-{1}.{2}-{3}", element.Parent.Designation, element.Parent.Revision, element.Designation, element.Revision));

            if (!_recordManager.IsRecordExists(RootElements.FirstOrDefault().Designation, element.Designation, element.Revision))
            {

                excelWriter.WriteCell(worksheet, row, 26, element.PartFile);
                excelWriter.WriteCell(worksheet, row, 27, element.AssemblyFile);

                if (element.DrawingFile != null && !element.DrawingFile.Contains(".dwg"))
                    excelWriter.WriteCell(worksheet, row, 28, element.DrawingFile);

                excelWriter.WriteCell(worksheet, row, 29, element.EMDrawingFile);
                excelWriter.WriteCell(worksheet, row, 30, element.EADrawingFile);
                excelWriter.WriteCell(worksheet, row, 31, element.REDrawingFile);

                if (element.DocFile != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.DocFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 19, element.DocFile);
                        excelWriter.WriteCell(worksheet, row, 43, Path.GetFileNameWithoutExtension(element.DocFile));
                        designationsWithFiles.Add(cachedRow);
                    }
                }


                if (element.DocxFile != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.DocxFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 20, element.DocxFile);
                        excelWriter.WriteCell(worksheet, row, 43, Path.GetFileNameWithoutExtension(element.DocxFile));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.PDFFile != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.PDFFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 24, element.PDFFile);
                        excelWriter.WriteCell(worksheet, row, 40, Path.GetFileNameWithoutExtension(element.PDFFile));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.ExcelFile != null)
                {

                    var cachedRow = string.Format("{0}-{1}", element.ExcelFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        if (element.ExcelFile.Contains(".xlsm"))
                        {
                            excelWriter.WriteCell(worksheet, row, 72, element.ExcelFile);
                            excelWriter.WriteCell(worksheet, row, 73, Path.GetFileNameWithoutExtension(element.ExcelFile));
                            designationsWithFiles.Add(cachedRow);
                        }
                        else if (element.ExcelFile.Contains(".xlsx"))
                        {
                            excelWriter.WriteCell(worksheet, row, 21, element.ExcelFile);
                            excelWriter.WriteCell(worksheet, row, 44, Path.GetFileNameWithoutExtension(element.ExcelFile));
                            designationsWithFiles.Add(cachedRow);

                        }
                        else
                        {
                            excelWriter.WriteCell(worksheet, row, 72, element.ExcelFile);
                            excelWriter.WriteCell(worksheet, row, 73, Path.GetFileNameWithoutExtension(element.ExcelFile));
                            designationsWithFiles.Add(cachedRow);
                        }
                    }
                }

                if (element.ZipFile != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.ZipFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 33, element.ZipFile);
                        excelWriter.WriteCell(worksheet, row, 42, Path.GetFileNameWithoutExtension(element.ZipFile));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.JpegFile != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.JpegFile, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 32, element.JpegFile);
                        excelWriter.WriteCell(worksheet, row, 41, Path.GetFileNameWithoutExtension(element.JpegFile));

                        designationsWithFiles.Add(cachedRow);
                    }
                }


                if (element.PNG != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.PNG, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 53, element.PNG);
                        excelWriter.WriteCell(worksheet, row, 66, Path.GetFileNameWithoutExtension(element.PNG));

                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.Html != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.Html, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 46, element.Html);
                        excelWriter.WriteCell(worksheet, row, 59, Path.GetFileNameWithoutExtension(element.Html));
                        designationsWithFiles.Add(cachedRow);
                    }
                }


                if (element.STEP != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.STEP, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 47, element.STEP);
                        excelWriter.WriteCell(worksheet, row, 60, Path.GetFileNameWithoutExtension(element.STEP));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.PPT != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.PPT, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 48, element.PPT);
                        excelWriter.WriteCell(worksheet, row, 61, Path.GetFileNameWithoutExtension(element.PPT));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.PPTX != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.PPTX, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 49, element.PPTX);
                        excelWriter.WriteCell(worksheet, row, 62, Path.GetFileNameWithoutExtension(element.PPTX));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.TXT != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.TXT, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 50, element.TXT);
                        excelWriter.WriteCell(worksheet, row, 63, Path.GetFileNameWithoutExtension(element.TXT));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.GIF != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.GIF, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 52, element.GIF);
                        excelWriter.WriteCell(worksheet, row, 65, Path.GetFileNameWithoutExtension(element.GIF));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.TIF != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.TIF, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 54, element.TIF);
                        excelWriter.WriteCell(worksheet, row, 67, Path.GetFileNameWithoutExtension(element.TIF));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.DXF != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.DXF, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 56, element.DXF);
                        excelWriter.WriteCell(worksheet, row, 69, Path.GetFileNameWithoutExtension(element.DXF));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.BMP != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.BMP, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 55, element.BMP);
                        excelWriter.WriteCell(worksheet, row, 68, Path.GetFileNameWithoutExtension(element.BMP));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.MSG != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.MSG, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {
                        excelWriter.WriteCell(worksheet, row, 58, element.MSG);
                        excelWriter.WriteCell(worksheet, row, 71, Path.GetFileNameWithoutExtension(element.MSG));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                if (element.DWG != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.DWG, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 57, element.DWG);
                        excelWriter.WriteCell(worksheet, row, 70, Path.GetFileNameWithoutExtension(element.DWG));
                        designationsWithFiles.Add(cachedRow);
                    }
                }
                if (element.Multy != null)
                {
                    var cachedRow = string.Format("{0}-{1}", element.Multy, element.Designation);
                    if (!designationsWithFiles.Contains(cachedRow))
                    {

                        excelWriter.WriteCell(worksheet, row, 51, element.Multy);
                        excelWriter.WriteCell(worksheet, row, 64, Path.GetFileNameWithoutExtension(element.Multy));
                        designationsWithFiles.Add(cachedRow);
                    }
                }

                _recordManager.AddRecord(RootElements.FirstOrDefault().Designation, element.Designation, element.Revision);
            }
            else
            {

            }
            excelWriter.WriteCell(worksheet, row, 45, element.AddInfo);
        }

        public List<Element> FlattenElements(Element element)
        {
            List<Element> flatList = new List<Element>();

            flatList.Add(element);

            foreach (var child in element.Children)
            {
                flatList.AddRange(FlattenElements(child));
            }

            return flatList;
        }

        public bool CheckIncorrectFiles(List<Element> elements)
        {
            if (Warnings.warnings.Count == 0)
                return true;

            StringBuilder messageBuilder = new StringBuilder();

            // Формируем текст сообщения
            foreach (var element in Warnings.warnings)
            {
                messageBuilder.AppendLine(element);
            }

            string message = messageBuilder.ToString();
            
            // Запись в файл, если есть предупреждения
            string filePath = "warnings_log.txt"; // Имя файла (можно указать путь)
            string timestamp = $"Выгрузка сборки {elements.FirstOrDefault().Designation} от {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
            
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine(); // Добавляем пустую строку сверху
            logBuilder.AppendLine(timestamp);
            logBuilder.AppendLine(message); // Добавляем сами сообщения

            // Записываем в файл без перезаписи предыдущих данных
            File.AppendAllText(filePath, logBuilder.ToString(), Encoding.UTF8);

            // Вызов диалогового окна с кнопками "Продолжить" (OK) и "Отмена" (Cancel)
            MessageBoxResult result = MessageBox.Show(
                message,
                "Предупреждение",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question
            );

            // Если пользователь нажал "Отмена", просто выходим
            if (result == MessageBoxResult.Cancel)
                return false;

            return true;
        }
    }
}
