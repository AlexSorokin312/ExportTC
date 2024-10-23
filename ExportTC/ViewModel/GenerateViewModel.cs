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
            // Получаем путь к рабочему столу текущего пользователя
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = Path.Combine(desktopPath, "Output.xlsm");

            // Извлечение шаблона Excel из ресурсов и копирование его в выходной файл
            using (var resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/HENKON.xlsm")).Stream)
            {
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }

            // Используем созданный файл для записи данных
            using (var excelWriter = new ExcelWriter(outputPath))
            {
                var worksheet = excelWriter.GetWorksheet(2);
                int row = 3;
                var elements = _assembly.Elements;
                foreach (var element in elements)
                {

                    excelWriter.WriteCell(worksheet, row, 3, element.Designation);
                    excelWriter.WriteCell(worksheet, row, 4, element.Name);
                    excelWriter.WriteCell(worksheet, row, 5, element.Quantity);
                    if (_initialData.IsCheckedMakeBuy)
                        excelWriter.WriteCell(worksheet, row, 2, element.Type);
                    excelWriter.WriteCell(worksheet, row, 10, element.Revision);

                    if (element.Parent != null)
                        excelWriter.WriteCell(worksheet, row, 1, element.Parent.Designation);

                    var fileName = element.FileName;
                    if (string.IsNullOrEmpty(fileName) || element.Designation == null)
                    {
                        excelWriter.WriteCell(worksheet, row, 26, element.Designation + ".SLDASM");
                        row++;
                        continue;
                    }

                    if (fileName.Contains("SLDPRT") || fileName.Contains("sldprt"))
                        excelWriter.WriteCell(worksheet, row, 25, element.FileName);
                    if (fileName.Contains("dwg") || fileName.Contains("DWG"))
                        excelWriter.WriteCell(worksheet, row, 27, element.FileName);
                    if (fileName.Contains("jpg") || fileName.Contains("JPG"))
                        excelWriter.WriteCell(worksheet, row, 28, element.FileName);
                    row++;
                }

                excelWriter.Save();
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
        }

    }
}
