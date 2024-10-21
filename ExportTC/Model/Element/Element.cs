using ExportTC.Constants;

namespace HenconExport.Model.Elemnts
{
    public class Element
    {
        public string Pos { get; set; }
        public string? Designation { get; set; }
        public string? Quantity { get; set; }
        public string? Name { get; set; }
        public string? MakeOrBuy { get; set; }
        public string? ProductStatus { get; set; }
        public string? Revision { get; set; }
        public string? Type { get; set; }
        public string? FileName { get; set; }
        public string? Drawing { get; set; }
        public Element Parent { get; set; }
        public List<Element>? Children { get; set; } = new List<Element>();

        public Element(string designation, string assembly)
        {
            Designation = designation;
            Name = assembly;
        }

        public Element()
        {
            Type = ElementConstants.DEFAULT_TYPE;   
        }

        public Element(string designation, string assembly, string pos, string quantity, string makeOrBuy, string revision) : this(designation, assembly)
        {
            Pos = pos;
            Quantity = quantity;
            MakeOrBuy = makeOrBuy;
            Revision = revision;
        }
    }
}
