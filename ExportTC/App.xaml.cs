using ExportTC.Model;
using ExportTC.Model.Factories;
using ExportTC.Services;
using ExportTC.View;
using ExportTC.ViewModel;
using HenconExport;
using HenconExport.Model.ExcelHandler;
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
            // Регистрация ваших контролов и ViewModel
            services.AddSingleton<InitialData>();
            services.AddSingleton<IExcelReaderFactory, ExcelReaderFactory>();
            services.AddSingleton<IHtmlReaderFactory, HtmReaderFactory>();
            services.AddTransient<PathViewModel>();
            services.AddTransient<ExcelViewModel>();
            services.AddTransient<GenerateViewModel>();
            services.AddSingleton<PathPanel>();
            services.AddSingleton<ExcelPanel>();
            services.AddSingleton<MainWindow>();
            services.AddTransient<IExcelReader, ExcelReader>();
            services.AddSingleton<ExcelDataExractor>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IFileSearchService, FileSearchService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IInitialDataSetter, ExcelDataImporter>();

            ServiceProvider = services.BuildServiceProvider();

            // Создание и показ основного окна
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
