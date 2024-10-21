using CommunityToolkit.Mvvm.ComponentModel;
using ExportTC.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ExportTC.ViewModel
{
    public partial class ExcelViewModel : ObservableObject
    {
        [ObservableProperty]
        private int? _headerRowNumber;

        [ObservableProperty]
        private int? _endRow;

        [ObservableProperty]
        private string? _productIDCell;

        [ObservableProperty]
        private string? _productNameCell;

        [ObservableProperty]
        private string? _designationColumn;

        [ObservableProperty]
        private string? _positionColumn;

        [ObservableProperty]
        private string? _quantityColumn;

        [ObservableProperty]
        private string? _descriptionColumn;

        [ObservableProperty]
        private string? _makeBuyColumn;

        [ObservableProperty]
        private string? _materialColumn;

        [ObservableProperty]
        private string? _revisionColumn;

        [ObservableProperty]
        private bool? _isCheckedPosition;

        [ObservableProperty]
        private bool? _isCheckedDesignation;

        [ObservableProperty]
        private bool? _isCheckedQuantity;

        [ObservableProperty]
        private bool? _isCheckedDescription;

        [ObservableProperty]
        private bool? _isCheckedMakeBuy;

        [ObservableProperty]
        private bool? _isCheckedMaterial;
        [ObservableProperty]
        private bool? _isCheckedRevision;

        private readonly InitialData _initialData;

        public ExcelViewModel()
        {
            _initialData = App.ServiceProvider.GetService<InitialData>();

            HeaderRowNumber = _initialData.HeaderRow;
            ProductIDCell = _initialData.ProductIDCell;
            ProductNameCell = _initialData.ProductNameCell;
            EndRow = _initialData.EndRow;

            DesignationColumn = _initialData.DesignationColumn;
            PositionColumn = _initialData.PositionColumn;
            QuantityColumn = _initialData.QuantityColumn;
            DescriptionColumn = _initialData.DescriptionColumn;
            MakeBuyColumn = _initialData.MakeBuyColumn;
            MaterialColumn = _initialData.MaterialColumn;
            RevisionColumn = _initialData.RevisionColumn;

            IsCheckedMakeBuy = _initialData.IsCheckedMakeBuy;
            IsCheckedMaterial = _initialData.IsCheckedMaterial;
            IsCheckedDescription = _initialData.IsCheckedDescription;
            IsCheckedDesignation = _initialData.IsCheckedDesignation;
            IsCheckedPosition = _initialData.IsCheckedPosition;
            IsCheckedQuantity = _initialData.IsCheckedQuantity;
            IsCheckedRevision = _initialData.IsCheckedRevision;
        }

        partial void OnHeaderRowNumberChanged(int? oldValue, int? newValue)
        {
            _initialData.HeaderRow = newValue ?? 0; 
        }

        partial void OnEndRowChanged(int? oldValue, int? newValue)
        {
            _initialData.EndRow = newValue ?? 0;
        }

        partial void OnProductIDCellChanged(string? oldValue, string? newValue)
        {
            _initialData.ProductIDCell = newValue;
        }

        partial void OnProductNameCellChanged(string? oldValue, string? newValue)
        {
            _initialData.ProductNameCell = newValue;
        }

        partial void OnDesignationColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.DesignationColumn = newValue;
        }

        partial void OnPositionColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.PositionColumn = newValue;
        }

        partial void OnQuantityColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.QuantityColumn = newValue;
        }

        partial void OnDescriptionColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.DescriptionColumn = newValue;
        }

        partial void OnMakeBuyColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.MakeBuyColumn = newValue;
        }

        partial void OnMaterialColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.MaterialColumn = newValue;
        }


        partial void OnIsCheckedPositionChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedPosition = newValue;
        }

        partial void OnIsCheckedMakeBuyChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedMakeBuy = newValue;
        }

        partial void OnIsCheckedMaterialChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedMaterial = newValue;
        }
    }
}
