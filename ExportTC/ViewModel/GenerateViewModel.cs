using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExportTC.Model;
using HenconExport.Model.Elemnts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExportTC.ViewModel
{
    internal class GenerateViewModel : ObservableObject
    {
        private InitialData _initialData;
        private Assembly _assembly;
        public ObservableCollection<Element> RootElements { get; private set; } = new ObservableCollection<Element>();
        public GenerateViewModel()
        {
            BrowseDirectoryCommand = new RelayCommand(Startprocess);
            _initialData = App.ServiceProvider.GetService<InitialData>();
        }

        public ICommand BrowseDirectoryCommand { get; }

        public void Startprocess()
        {
            var assemblyFiller = App.ServiceProvider.GetService<AssemblyConstructor>();

            _assembly = assemblyFiller.GetAssembly(_initialData);
            var rootElements = _assembly.GetRootElements();

            foreach (var element in rootElements)
            {
                RootElements.Add(element); 
            }
        }
    } 
}
