using CommunityToolkit.Mvvm.ComponentModel;
using ExportTC.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ExportTC.ViewModel
{
    public partial class ExcelViewModel : ObservableObject
    {
        [ObservableProperty] private int? _headerRowNumber;
        [ObservableProperty] private int? _endRow;
        [ObservableProperty] private string? _productIDCell;
        [ObservableProperty] private string? _productNameCell;
        [ObservableProperty] private string? _designationColumn;
        [ObservableProperty] private string? _positionColumn;
        [ObservableProperty] private string? _quantityColumn;
        [ObservableProperty] private string? _descriptionColumn;
        [ObservableProperty] private string? _makeBuyColumn;
        [ObservableProperty] private string? _materialColumn;
        [ObservableProperty] private string? _revisionColumn;
        [ObservableProperty] private string? _costtypeColumn;
        [ObservableProperty] private string? _spareColumn;
        [ObservableProperty] private string? _itemCodeSupplierColumn;
        [ObservableProperty] private string? _addInfoColumn;

        private readonly InitialData _initialData;

        public ExcelViewModel()
        {
            _initialData = App.ServiceProvider.GetService<InitialData>()
                           ?? throw new InvalidOperationException(ErrorMessages.InitialDataServiceError);
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            HeaderRowNumber = _initialData.HeaderRow;
            EndRow = _initialData.EndRow;
            ProductIDCell = _initialData.ProductIDCell;
            ProductNameCell = _initialData.ProductNameCell;
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
        }

        partial void OnHeaderRowNumberChanged(int? oldValue, int? newValue) => _initialData.HeaderRow = newValue ?? 0;
        partial void OnEndRowChanged(int? oldValue, int? newValue) => _initialData.EndRow = newValue ?? 0;
        partial void OnProductIDCellChanged(string? oldValue, string? newValue) => _initialData.ProductIDCell = newValue;
        partial void OnProductNameCellChanged(string? oldValue, string? newValue) => _initialData.ProductNameCell = newValue;
        partial void OnDesignationColumnChanged(string? oldValue, string? newValue) => _initialData.DesignationColumn = newValue;
        partial void OnPositionColumnChanged(string? oldValue, string? newValue) => _initialData.PositionColumn = newValue;
        partial void OnQuantityColumnChanged(string? oldValue, string? newValue) => _initialData.QuantityColumn = newValue;
        partial void OnDescriptionColumnChanged(string? oldValue, string? newValue) => _initialData.DescriptionColumn = newValue;
        partial void OnMakeBuyColumnChanged(string? oldValue, string? newValue) => _initialData.MakeBuyColumn = newValue;
        partial void OnMaterialColumnChanged(string? oldValue, string? newValue) => _initialData.MaterialColumn = newValue;
        partial void OnRevisionColumnChanged(string? oldValue, string? newValue) => _initialData.RevisionColumn = newValue;
        partial void OnCosttypeColumnChanged(string? oldValue, string? newValue) => _initialData.CosttypeColumn = newValue;
        partial void OnSpareColumnChanged(string? oldValue, string? newValue) => _initialData.SpareColumn = newValue;
        partial void OnItemCodeSupplierColumnChanged(string? oldValue, string? newValue) => _initialData.ItemCodeSupplierColumn = newValue;
        partial void OnAddInfoColumnChanged(string? oldValue, string? newValue) => _initialData.AddInfoColumn = newValue;
    }
}
