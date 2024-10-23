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
        private string? _costtypeColumn;

        [ObservableProperty]
        private string? _spareColumn;

        [ObservableProperty]
        private string? _itemCodeSupplierColumn;

        [ObservableProperty]
        private string? _addInfoColumn;

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

        [ObservableProperty]
        private bool? _isCheckedCosttype;

        [ObservableProperty]
        private bool? _isCheckedSpare;

        [ObservableProperty]
        private bool? _isCheckedItemCodeSupplier;

        [ObservableProperty]
        private bool? _isCheckedAddInfo;

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
            CosttypeColumn = _initialData.CosttypeColumn;
            SpareColumn = _initialData.SpareColumn;
            ItemCodeSupplierColumn = _initialData.ItemCodeSupplierColumn;
            AddInfoColumn = _initialData.AddInfoColumn;

            IsCheckedMakeBuy = _initialData.IsCheckedMakeBuy;
            IsCheckedMaterial = _initialData.IsCheckedMaterial;
            IsCheckedDescription = _initialData.IsCheckedDescription;
            IsCheckedDesignation = _initialData.IsCheckedDesignation;
            IsCheckedPosition = _initialData.IsCheckedPosition;
            IsCheckedQuantity = _initialData.IsCheckedQuantity;
            IsCheckedRevision = _initialData.IsCheckedRevision;
            IsCheckedCosttype = _initialData.IsCheckedCosttype;
            IsCheckedSpare = _initialData.IsCheckedSpare;
            IsCheckedItemCodeSupplier = _initialData.IsCheckedItemCodeSupplier;
            IsCheckedAddInfo = _initialData.IsCheckedAddInfo;
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

        partial void OnRevisionColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.RevisionColumn = newValue;
        }

        partial void OnCosttypeColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.CosttypeColumn = newValue;
        }

        partial void OnSpareColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.SpareColumn = newValue;
        }

        partial void OnItemCodeSupplierColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.ItemCodeSupplierColumn = newValue;
        }

        partial void OnAddInfoColumnChanged(string? oldValue, string? newValue)
        {
            _initialData.AddInfoColumn = newValue;
        }

        partial void OnIsCheckedPositionChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedPosition = newValue ?? false;
        }

        partial void OnIsCheckedMakeBuyChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedMakeBuy = newValue ?? false;
        }

        partial void OnIsCheckedMaterialChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedMaterial = newValue ?? false;
        }

        partial void OnIsCheckedRevisionChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedRevision = newValue ?? false;
        }

        partial void OnIsCheckedCosttypeChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedCosttype = newValue ?? false;
        }

        partial void OnIsCheckedSpareChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedSpare = newValue ?? false;
        }

        partial void OnIsCheckedItemCodeSupplierChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedItemCodeSupplier = newValue ?? false;
        }

        partial void OnIsCheckedAddInfoChanged(bool? oldValue, bool? newValue)
        {
            _initialData.IsCheckedAddInfo = newValue ?? false;
        }
    }
}
