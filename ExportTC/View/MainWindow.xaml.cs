using ExportTC.View;
using ExportTC.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ExportTC
{
    public partial class MainWindow : Window
    {
        private readonly PathPanel _pathPanel;
        private readonly ExcelPanel _excelPanel;

        public MainWindow()
        {
            InitializeComponent();
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/logo.ico"));
            _pathPanel = App.ServiceProvider.GetService<PathPanel>();
            _excelPanel = App.ServiceProvider.GetService<ExcelPanel>();
        }

        private void NavogateToExcelButton(object sender, RoutedEventArgs e)
        {
            var excelViewModel = App.ServiceProvider.GetService<ExcelViewModel>();
            var excelPanel = new ExcelPanel { DataContext = excelViewModel };
            ContentArea.Content = excelPanel;
        }

        private void NavigateToPathPanel(object sender, RoutedEventArgs e)
        {
            var pathViewModel = App.ServiceProvider.GetService<PathViewModel>();
            var pathPanel = new PathPanel { DataContext = pathViewModel };
            ContentArea.Content = pathPanel;
        }

        private void NavigateToGeneratePanel(object sender, RoutedEventArgs e)
        {
            var pathViewModel = App.ServiceProvider.GetService<GenerateViewModel>();
            var pathPanel = new GenerateView { DataContext = pathViewModel };
            ContentArea.Content = pathPanel;
        }
    }
}
