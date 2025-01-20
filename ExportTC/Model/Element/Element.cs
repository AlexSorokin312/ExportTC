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
        public string? TCType { get; set; }
        public string? TreeType { get; set; }
        public string? DrawingFile { get; set; }

        public string? EADrawingFile;

        public string? REDrawingFile { get; set; }

        public string? EMDrawingFile;
        public string? ZipFile { get; set; }
        public string? PartFile { get; set; }
        public string? AssemblyFile { get; set; }
        public string? DocFile { get; set; }
        public string? DocxFile { get; set; }
        public string? JpegFile { get; set; }
        public string? PDFFile { get; set; }
        public string? Html { get; set; }
        public string? FileName { get; set; }
        public string? DrawingIcon { get; set; }
        public Element Parent { get; set; }
        public bool Root { get; set; }
        public List<Element>? Children { get; set; } = new List<Element>();

        public string? ItemCodeSupplier { get; set; }
        public string? Costtype { get; set; }
        public string? Spare { get; set; }
        public string? AddInfo { get; set; }
        public string? ExcelFile { get; set; }

        public Element(string designation, string assembly)
        {
            Designation = designation;
            Name = assembly;
            Revision = "00";
        }

        public Element()
        {
            Revision = "00";
            TCType = ElementConstants.DEFAULT_TYPE;
        }

        public Element(string designation, string assembly, string pos, string quantity, string makeOrBuy, string revision) : this(designation, assembly)
        {
            Pos = pos;
            Quantity = quantity;
            MakeOrBuy = makeOrBuy;
            Revision = revision ?? "00";
        }
    }
}
