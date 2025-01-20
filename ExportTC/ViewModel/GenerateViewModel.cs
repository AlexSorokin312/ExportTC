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
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace ExportTC.ViewModel
{
    internal class GenerateViewModel : ObservableObject
    {
        private InitialData _initialData;
        private IFileSearchService _fileSearchService;
        private Assembly _assembly;
        public ObservableCollection<Element> RootElements { get; private set; } = new ObservableCollection<Element>();

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
                return;
            IsProgressVisible = true;
            ProgressValue = 0;


            string savePath = _initialData.SavePath;
            if (File.Exists(Path.Combine(savePath, "Hencon_Impl.xlsm"))) ;
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
                    var flattenElements = FlattenElements(_assembly.GetRootElement());
                    _assembly.Sort(flattenElements);
                    int totalElements = flattenElements.Count;

                    for (int i = 0; i < totalElements; i++)
                    {
                        if (!string.IsNullOrEmpty(flattenElements[i].FileName))
                        {
                            if (flattenElements[i].FileName.Contains("dwg"))
                                continue;
                        }
                        WriteElementToExcel(excelWriter, worksheet, row, flattenElements[i]);
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

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private void WriteElementToExcel(ExcelWriter excelWriter, ExcelWorksheet worksheet, int row, Element element)
        {

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
            excelWriter.WriteCell(worksheet, row, 15, element.Designation + "-" + element.Revision);
            if (element.Parent != null)
                excelWriter.WriteCell(worksheet, row, 16, string.Format("{0}-{1}.{2}-{3}", element.Parent.Designation, element.Parent.Revision, element.Designation, element.Revision));
            if (!string.IsNullOrEmpty(element.DocFile))
                excelWriter.WriteCell(worksheet, row, 19, element.DocFile);
            if (!string.IsNullOrEmpty(element.DocxFile))
                excelWriter.WriteCell(worksheet, row, 20, element.DocxFile);
            excelWriter.WriteCell(worksheet, row, 21, element.ExcelFile);
            excelWriter.WriteCell(worksheet, row, 24, element.PDFFile);
            excelWriter.WriteCell(worksheet, row, 26, element.PartFile);
            excelWriter.WriteCell(worksheet, row, 27, element.AssemblyFile);

            excelWriter.WriteCell(worksheet, row, 28, element.DrawingFile);
            excelWriter.WriteCell(worksheet, row, 29, element.EMDrawingFile);
            excelWriter.WriteCell(worksheet, row, 30, element.EADrawingFile);
            excelWriter.WriteCell(worksheet, row, 31, element.REDrawingFile);
            excelWriter.WriteCell(worksheet, row, 32, element.JpegFile);
            excelWriter.WriteCell(worksheet, row, 33, element.ZipFile);
            excelWriter.WriteCell(worksheet, row, 34, element.Costtype);
            excelWriter.WriteCell(worksheet, row, 35, element.MakeOrBuy);
            excelWriter.WriteCell(worksheet, row, 36, element.Spare);
            excelWriter.WriteCell(worksheet, row, 37, element.ItemCodeSupplier);
            excelWriter.WriteCell(worksheet, row, 38, element.TreeType);

            excelWriter.WriteCell(worksheet, row, 40, Path.GetFileNameWithoutExtension(element.PDFFile));
            excelWriter.WriteCell(worksheet, row, 41, Path.GetFileNameWithoutExtension(element.JpegFile));
            excelWriter.WriteCell(worksheet, row, 42, Path.GetFileNameWithoutExtension(element.ZipFile));
            if (!string.IsNullOrWhiteSpace(element.DocFile))
                excelWriter.WriteCell(worksheet, row, 43, Path.GetFileNameWithoutExtension(element.DocFile));
            if (!string.IsNullOrWhiteSpace(element.DocxFile))
                excelWriter.WriteCell(worksheet, row, 43, Path.GetFileNameWithoutExtension(element.DocxFile));
            excelWriter.WriteCell(worksheet, row, 44, Path.GetFileNameWithoutExtension(element.ExcelFile));
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
    }
}
