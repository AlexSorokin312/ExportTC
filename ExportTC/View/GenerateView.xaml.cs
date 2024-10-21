using ExportTC.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace ExportTC.View
{
    /// <summary>
    /// Interaction logic for GenerateViewModel.xaml
    /// </summary>
    public partial class GenerateView : UserControl
    {
        private GenerateViewModel _viewModel;

        public GenerateView()
        {
            _viewModel = App.ServiceProvider.GetService<GenerateViewModel>();
             DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
