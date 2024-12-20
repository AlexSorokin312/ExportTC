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
        private string? _saveFilePath;

        [ObservableProperty]
        private string? _directoryPath;

        [ObservableProperty]
        private string? _excelFilePath;

        [ObservableProperty]
        private string? _htmFilePath;

        public ICommand BrowseDirectoryCommand { get; }
        public ICommand BrowseExcelFileCommand { get; }
        public ICommand BrowseHtmFileCommand { get; }
        public ICommand BrowseSavePathCommand { get; }
        public ICommand BrowseGenericFileCommand { get; }

        private readonly Lazy<InitialData> _initialData;
        private readonly Lazy<IFileSearchService> _fileSearchService;
        private readonly Lazy<IFileDialogService> _fileDialogService;

        public PathViewModel()
        {
            try
            {
                _initialData = new Lazy<InitialData>(() =>
                    App.ServiceProvider.GetService<InitialData>()
                    ?? throw new InvalidOperationException(ErrorMessages.InitialDataServiceError));

                _fileSearchService = new Lazy<IFileSearchService>(() =>
                    App.ServiceProvider.GetService<IFileSearchService>()
                    ?? throw new InvalidOperationException(ErrorMessages.FileSearchServiceError));

                _fileDialogService = new Lazy<IFileDialogService>(() =>
                    App.ServiceProvider.GetService<IFileDialogService>()
                    ?? throw new InvalidOperationException(ErrorMessages.FileDialogServiceError));

                _directoryPath = _initialData.Value.BaseDirectory;
                _excelFilePath = _initialData.Value.ExcelFile;
                _htmFilePath = _initialData.Value.HtmlFile;
                _saveFilePath = _initialData.Value.SavePath;

                BrowseDirectoryCommand = new RelayCommand(OpenCommonDirectoryDialog);
                BrowseExcelFileCommand = new RelayCommand(OpenExcelFileDialog);
                BrowseHtmFileCommand = new RelayCommand(OpenHtmFileDialog);
                BrowseSavePathCommand = new RelayCommand(OpenSaveFileDialog);
            }
            catch (Exception ex)
            {
                AppLogger.LogFatal(ex, ErrorMessages.ViewModelInitializationError);
                throw;
            }
        }

        partial void OnDirectoryPathChanged(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), ErrorMessages.InvalidDirectoryPath);

                ExcelFilePath = _fileSearchService.Value.FindFirstExcelFile(value);
                HtmFilePath = _fileSearchService.Value.FindHtmlFile(value);
                _initialData.Value.BaseDirectory = value;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.DirectoryPathChangeError);
            }
        }

        partial void OnExcelFilePathChanged(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), ErrorMessages.InvalidExcelFilePath);

                _initialData.Value.ExcelFile = value;
                var _initialDataSetter = App.ServiceProvider.GetService<IInitialDataSetter>()
                    ?? throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError);

                _initialDataSetter.PrepareData(value);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.ExcelFilePathChangeError);
            }
        }

        partial void OnHtmFilePathChanged(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), ErrorMessages.InvalidHtmFilePath);

                _initialData.Value.HtmlFile = value;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.HtmlFilePathChangeError);
            }
        }

        private void OpenExcelFileDialog()
        {
            try
            {
                var selectedFile = _fileDialogService.Value.OpenExcelFile();
                if (!string.IsNullOrWhiteSpace(selectedFile))
                {
                    ExcelFilePath = selectedFile;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.ExcelFileDialogError);
            }
        }

        private void OpenHtmFileDialog()
        {
            try
            {
                var selectedFile = _fileDialogService.Value.OpenHtmlFile();
                if (!string.IsNullOrWhiteSpace(selectedFile))
                {
                    HtmFilePath = selectedFile;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.HtmFileDialogError);
            }
        }

        private void OpenCommonDirectoryDialog()
        {
            try
            {
                var selectedDirectory = _fileDialogService.Value.OpenDirectory();
                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    DirectoryPath = selectedDirectory;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.DirectoryDialogError);
            }
        }

        private void OpenSaveFileDialog()
        {
            try
            {
                var selectedDirectory = _fileDialogService.Value.OpenDirectory();
                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    SaveFilePath = selectedDirectory;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.DirectoryDialogError);
            }
        }
    }
}
