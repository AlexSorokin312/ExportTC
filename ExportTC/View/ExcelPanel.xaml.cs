using ExportTC.ViewModel;
using System.Windows.Controls;

namespace ExportTC.View
{
    public partial class ExcelPanel : UserControl
    {
        private ExcelViewModel _viewModel;
        public ExcelPanel()
        {
            _viewModel = new ExcelViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }
    }
}
