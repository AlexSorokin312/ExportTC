using ExportTC.Constants;
using HenconExport;

namespace ExportTC.Model
{
    public class InitialData
    {
        #region Path
        public string BaseDirectory { get; set; }
        public string HtmlFile { get; set; }
        public string ExcelFile { get; set; }
        #endregion

        #region Excel
        public int SheetNumber { get; set; }
        public string? ProductIDCell { get; set; }
        public string? ProductNameCell { get; set; }
        public int HeaderRow { get; set; }
        public int StartRow => HeaderRow+1;
        public int EndRow { get; set; }

        public string? PositionColumn { get; set; }
        public string? DesignationColumn { get; set; }
        public string? QuantityColumn { get; set; }
        public string? DescriptionColumn { get; set; }
        public string? MakeBuyColumn { get; set; }
        public string? MaterialColumn { get; set; }
        public string? RevisionColumn { get; set; }

        public bool? IsCheckedPosition { get; set; }
        public bool? IsCheckedDesignation { get; set; }
        public bool? IsCheckedQuantity { get; set; }
        public bool? IsCheckedDescription { get; set; }
        public bool? IsCheckedMakeBuy { get; set; }
        public bool? IsCheckedMaterial { get; set; }
        public bool? IsCheckedRevision { get; set; }

        #endregion

        public InitialData()
        {
            SheetNumber = DefaultReadSettings.LIST_NUMBER_DEFAULT;
            HeaderRow = DefaultReadSettings.HEADER_ROW_NUMBER;
            ProductIDCell = DefaultReadSettings.DESIGNAION_ID_DEFAULT;
            ProductNameCell = DefaultReadSettings.PRODUCT_NAME_DEFAULT;

            IsCheckedDescription = true;
            IsCheckedDesignation = true;
            IsCheckedMakeBuy = true;
            IsCheckedMaterial = false;
            IsCheckedPosition = false;
            IsCheckedQuantity = true;
            IsCheckedRevision = true;
        }

    }

}
