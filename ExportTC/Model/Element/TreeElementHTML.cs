namespace HenconExport.Model.Elemnts
{

    public class TreeElementHTML : Element
    {
        public string? Status { get; set; }
        public string? Drawing { get; set; }
        public string? FileName { get; set; }
        public TreeElementHTML? Parent;
        public List<TreeElementHTML>? Children { get; set; } = new();

        public TreeElementHTML(string designation, string assembly) : base(designation, assembly){}

        public TreeElementHTML() {}
    }
}
