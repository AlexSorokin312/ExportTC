
using HenconExport.Model.Elemnts;

namespace MigrateData.Models
{
    /// <summary>
    /// Класс для хранения одной строки данных из Excel.
    /// </summary>
    public class NewDataItem
    {
        public string HierarchyPath { get; set; }
        public string UNIQUE_ID { get; set; }
        public string ID { get; set; }
        public string DESCRIPTION { get; set; }
        public string MAKE_BUY { get; set; }
        public string Quantity { get; set; }
        public string COSTTYPE { get; set; }
        public string SPARES { get; set; }
        public string REVISION { get; set; }
        public string ITEM_CODE_SUPPLIER { get; set; }
        public string ADD_INFO { get; set; }
        public string HENCON_STD { get; set; }
        public string Level { get; set; }
        public string Type { get; set; }
        public string FILE_NAME { get; set; }
        public string BOM { get; set; }
        public string LONG_PATH { get; set; }
        public NewDataItem Parent { get; set; }
        public bool Root { get; set; }
        public List<NewDataItem>? Children { get; set; } = new List<NewDataItem>();

        public NewDataItem() { }

        public override string ToString()
        {
            return $"{HierarchyPath} | {UNIQUE_ID} | {ID} | {DESCRIPTION}";
        }
    }
}
