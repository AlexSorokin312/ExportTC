public class ExcelElementConfig
{
    public string ExcelPath { get; set; }
    public int SheetNumber { get; set; }
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public string ProductIDCell { get; set; }
    public string ProductNameCell { get; set; }
    public string PositionColumn { get; set; }
    public string DesignationColumn { get; set; }
    public string DescriptionColumn { get; set; }
    public string QuantityColumn { get; set; }
    public string MakeOrBuyColumn { get; set; }
    public string RevisionColumn { get; set; }

    // Новые поля
    public string ItemCodeSupplier { get; set; }
    public string Costtype { get; set; }
    public string Spare { get; set; }
    public string AddInfo { get; set; }
}