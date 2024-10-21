using System.Windows;
using System.Windows.Controls;

namespace ExportTC.View.Custom
{
    public partial class ExcelRow : UserControl
    {
        public ExcelRow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ContentProperty =
    DependencyProperty.Register("Content", typeof(string), typeof(ExcelRow), new PropertyMetadata(string.Empty));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }


        public static readonly DependencyProperty DirectoryPathProperty =
    DependencyProperty.Register("DirectoryPath", typeof(string), typeof(ExcelRow), new PropertyMetadata(default(string)));

        public string DirectoryPath
        {
            get => (string)GetValue(DirectoryPathProperty);
            set => SetValue(DirectoryPathProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ExcelRow), new PropertyMetadata(default(bool?)));

        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
    }
}
