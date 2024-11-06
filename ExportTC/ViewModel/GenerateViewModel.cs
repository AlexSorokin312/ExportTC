using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExportTC.Model;
using HenconExport;
using HenconExport.Model.Elemnts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

        private string _selectedComboBoxItem;
        public string SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set => SetProperty(ref _selectedComboBoxItem, value);
        }

        public GenerateViewModel()
        {
            _initialData = App.ServiceProvider.GetService<InitialData>();
            _fileSearchService = App.ServiceProvider.GetService<IFileSearchService>();
            ComboBoxItems = new ObservableCollection<string> { 
                "Структура изделия (с матрицей)",
                "Структура изделия с заменами",
                "Структура (наборы данных)" };

            SelectedComboBoxItem = ComboBoxItems[1];

            DisplayTree();
            BrowseDirectoryCommand = new RelayCommand(SaveToExcelFile);
        }

        public ICommand BrowseDirectoryCommand { get; }

        public void DisplayTree()
        {
            var assemblyFiller = App.ServiceProvider.GetService<AssemblyConstructor>();

            try
            {
                _assembly = assemblyFiller.GetAssembly(_initialData);
                var rootElements = _assembly.GetRootElements();
                foreach (var element in rootElements)
                {
                    RootElements.Add(element);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SaveToExcelFile()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = Path.Combine(desktopPath, "Output.xlsm");

            using (var resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/HENKON_imp.xlsm")).Stream)
            {
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }

            using (var excelWriter = new ExcelWriter(outputPath))
            {
                var worksheet = excelWriter.GetWorksheet(2);
                int row = 3;
                var elements = _assembly.Elements;

                foreach (var element in elements)
                {
                    if (element.Parent != null)
                        excelWriter.WriteCell(worksheet, row, 1, element.Parent.Designation);

                    excelWriter.WriteCell(worksheet, row, 3, element.Designation);
                    excelWriter.WriteCell(worksheet, row, 4, element.Name);
                    excelWriter.WriteCell(worksheet, row, 5, element.Quantity);
                    excelWriter.WriteCell(worksheet, row, 2, "Элемент");
                    excelWriter.WriteCell(worksheet, row, 10, element.Revision);
                    excelWriter.WriteCell(worksheet, row, 31, element.Costtype);
                    excelWriter.WriteCell(worksheet, row, 32, element.MakeOrBuy);
                    excelWriter.WriteCell(worksheet, row, 33, element.Spare);
                    excelWriter.WriteCell(worksheet, row, 34, element.ItemCodeSupplier);
                    excelWriter.WriteCell(worksheet, row, 35, element.TreeType);
                    excelWriter.WriteCell(worksheet, row, 6, element.AddInfo);

                    excelWriter.WriteCell(worksheet, row, 27, element.AssemblyFile);
                    excelWriter.WriteCell(worksheet, row, 28, element.DrawingFile);
                    excelWriter.WriteCell(worksheet, row, 24, element.PDFFile);
                    excelWriter.WriteCell(worksheet, row, 30, element.ZipFile);
                    excelWriter.WriteCell(worksheet, row, 26, element.PartFile);
                    excelWriter.WriteCell(worksheet, row, 20, element.DocxFile);
                    excelWriter.WriteCell(worksheet, row, 19, element.DocFile);
                    excelWriter.WriteCell(worksheet, row, 29, element.JpegFile);
                    row++;
                }
                excelWriter.Save();
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
        }
    }
}
