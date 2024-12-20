using ExportTC.Constants;
using System.IO;

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
        public int StartRow => HeaderRow + 1;
        public int EndRow { get; set; }

        public string? PositionColumn { get; set; }
        public string? DesignationColumn { get; set; }
        public string? QuantityColumn { get; set; }
        public string? DescriptionColumn { get; set; }
        public string? MakeBuyColumn { get; set; }
        public string? MaterialColumn { get; set; }
        public string? RevisionColumn { get; set; }
        public string? CosttypeColumn { get; set; }
        public string? SpareColumn { get; set; }
        public string? ItemCodeSupplierColumn { get; set; }
        public string? AddInfoColumn { get; set; }

        public string PathFiles { get; set; }
        public string SavePath { get; set; }
        #endregion

        public InitialData()
        {
            SheetNumber = DefaultReadSettings.LIST_NUMBER_DEFAULT;
            HeaderRow = DefaultReadSettings.HEADER_ROW_NUMBER;
            ProductIDCell = DefaultReadSettings.DESIGNAION_ID_DEFAULT;
            ProductNameCell = DefaultReadSettings.PRODUCT_NAME_DEFAULT;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SavePath = Path.Combine(desktopPath);
        }
    }
}
