using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExportTC.Model;
using HenconExport.Model.Elemnts;
using HenconExport.Model.ExcelHandler;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExportTC.ViewModel
{
    internal class GenerateViewModel : ObservableObject
    {
        private InitialData _initialData;
        private ExcelDataExractor _excelHandler;

        public ObservableCollection<Element> RootElements { get; private set; } = new ObservableCollection<Element>();
        public GenerateViewModel()
        {
            BrowseDirectoryCommand = new RelayCommand(Startprocess);
            _initialData = App.ServiceProvider.GetService<InitialData>();
        }

        public ICommand BrowseDirectoryCommand { get; }

        public void Startprocess()
        {
            _excelHandler = App.ServiceProvider.GetService<ExcelDataExractor>();

            var assembly = _excelHandler.GetAllElements();
            var rootElements = assembly.GetRootElements();

            RootElements.Clear(); // Очистить существующие элементы
            foreach (var element in rootElements)
            {
                RootElements.Add(element); // Добавить новые корневые элементы
            }
        }


    }
}
