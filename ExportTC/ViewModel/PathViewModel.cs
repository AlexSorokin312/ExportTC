using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExportTC.Model;
using ExportTC.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace ExportTC.ViewModel
{
    public partial class PathViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _directoryPath;

        [ObservableProperty]
        private string? _excelFilePath;

        [ObservableProperty]
        private string? _htmFilePath;

        public ICommand BrowseDirectoryCommand { get; }
        public ICommand BrowseExcelFileCommand { get; }
        public ICommand BrowseHtmFileCommand { get; }

        private readonly InitialData _initialData;
        private readonly IFileSearchService _fileSearchService;
        private readonly IFileDialogService _fileDialogService;

        public PathViewModel()
        {
            _initialData = App.ServiceProvider.GetService<InitialData>();
            _fileSearchService = App.ServiceProvider.GetService<IFileSearchService>();
            _fileDialogService = App.ServiceProvider.GetService<IFileDialogService>();

            _directoryPath = _initialData.BaseDirectory;
            _excelFilePath = _initialData.ExcelFile;
            _htmFilePath = _initialData.HtmlFile;

            BrowseDirectoryCommand = new RelayCommand(OpenDirectoryDialog);
            BrowseExcelFileCommand = new RelayCommand(OpenExcelFileDialog);
            BrowseHtmFileCommand = new RelayCommand(OpenHtmFileDialog);
        }

        partial void OnDirectoryPathChanged(string value)
        {

            ExcelFilePath = _fileSearchService.FindFirstExcelFile(value);
            HtmFilePath = _fileSearchService.FindHtmlFile(value);

            _initialData.BaseDirectory = value;

        }

        partial void OnExcelFilePathChanged(string value)
        {
            _initialData.ExcelFile = value;
            var _initialDataSetter = App.ServiceProvider.GetService<IInitialDataSetter>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                _initialDataSetter.PrepareData(value);
            }
        }

        partial void OnHtmFilePathChanged(string value)
        {
            _initialData.HtmlFile = value;
        }

        private void OpenExcelFileDialog()
        {
            var selectedFile = _fileDialogService.OpenExcelFile();
            if (selectedFile != null)
            {
                ExcelFilePath = selectedFile;
            }
        }

        private void OpenHtmFileDialog()
        {
            var selectedFile = _fileDialogService.OpenHtmlFile();
            if (selectedFile != null)
            {
                HtmFilePath = selectedFile;
            }
        }

        private void OpenDirectoryDialog()
        {
            var selectedDirectory = _fileDialogService.OpenDirectory();
            if (selectedDirectory != null)
            {
                DirectoryPath = selectedDirectory;
            }
        }
    }
}
