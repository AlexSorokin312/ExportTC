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
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IExcelReaderFactory, ExcelReaderFactory>();
            services.AddSingleton<IHtmlReaderFactory, HtmReaderFactory>();
            services.AddSingleton<IFileSearchService, FileSearchService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IInitialDataSetter, ExcelDataImporter>();
            services.AddSingleton<IElementTreeBuilder, ElementTreeBuilder>();
            services.AddSingleton<IHtmlReader, HtmlReader>();
            services.AddTransient<IExcelReader, ExcelReader>();
            services.AddSingleton<InitialData>();
            services.AddTransient<PathViewModel>();
            services.AddTransient<ExcelViewModel>();
            services.AddTransient<GenerateViewModel>();
            services.AddSingleton<PathPanel>();
            services.AddSingleton<ExcelPanel>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<HtmlElementParser>();
            services.AddSingleton<ExcelElementParser>();
            services.AddSingleton<AssemblyConstructor>();
        }
    }
}
