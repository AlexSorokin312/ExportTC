using ExportTC.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExportTC.View
{
    public partial class PathPanel : UserControl
    {
        private PathViewModel _viewModel;

        public PathPanel()
        {
            _viewModel = App.ServiceProvider.GetService<PathViewModel>();
            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
