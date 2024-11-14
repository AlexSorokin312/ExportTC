using ExportTC.Interfaces;
using ExportTC.Model;
using ExportTC.Model.ElementParcers;
using ExportTC.Model.Factories;
using ExportTC.Services;
using ExportTC.View;
using ExportTC.ViewModel;
using HenconExport;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System.Windows;

namespace ExportTC
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var services = new ServiceCollection();
            try
            {
                ConfigureServices(services);
                ServiceProvider = services.BuildServiceProvider();

                var mainWindow = ServiceProvider.GetService<MainWindow>();
                if (mainWindow == null)
                {
                    AppLogger.LogFatal(new Exception("MainWindow не был зарегистрирован."), ErrorMessages.MainWindowStartupError);
                    throw new InvalidOperationException("MainWindow не был зарегистрирован.");
                }
                mainWindow.Show();

            }
            catch (Exception ex)
            {
                AppLogger.LogFatal(ex, ErrorMessages.ApplicationInitializationError);
                throw;
            }

            base.OnStartup(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            AddFactoryServices(services);
            AddReaderServices(services);
            AddViewModelServices(services);
            AddPanelServices(services);
            AddInitialDataServices(services);
            AddParserServices(services);
        }

        private void AddFactoryServices(ServiceCollection services)
        {
            services.AddSingleton<IExcelReaderFactory, ExcelReaderFactory>();
            services.AddSingleton<IHtmlReaderFactory, HtmReaderFactory>();
        }

        private void AddReaderServices(ServiceCollection services)
        {
            services.AddSingleton<IFileSearchService, FileSearchService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IHtmlReader, HtmlReader>();
            services.AddSingleton<IElementTreeBuilder, ElementTreeBuilder>();
            services.AddTransient<IExcelReader, ExcelReader>();
        }

        private void AddInitialDataServices(ServiceCollection services)
        {
            services.AddSingleton<InitialData>();
            services.AddSingleton<IInitialDataSetter, ExcelDataImporter>();
        }

        private void AddViewModelServices(ServiceCollection services)
        {
            services.AddTransient<PathViewModel>();
            services.AddTransient<ExcelViewModel>();
            services.AddTransient<GenerateViewModel>();
        }

        private void AddPanelServices(ServiceCollection services)
        {
            services.AddSingleton<PathPanel>();
            services.AddSingleton<ExcelPanel>();
            services.AddSingleton<MainWindow>();
        }

        private void AddParserServices(ServiceCollection services)
        {
            services.AddSingleton<HtmlElementParser>();
            services.AddSingleton<ExcelElementParser>();
            services.AddSingleton<AssemblyConstructor>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                AppLogger.LogInformation(ErrorMessages.ApplicationExitInfo);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.ApplicationExitError);
            }
            finally
            {
                AppLogger.CloseLogger();
            }

            base.OnExit(e);
        }
    }
}
