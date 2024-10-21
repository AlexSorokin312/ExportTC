namespace ExportTC.Constants
{
    public static class CommonConstants
    {
        public const string DirectoryDialogDescription = "Выберите папку";
        public const string ExcelFileDialogFilter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";
        public const string HtmFileDialogFilter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";

        public static Dictionary<string, string> Replacments;

        public static Dictionary<string, string> BomMakePictures;

        public static List<string> ElementTypePictures;
        public static List<string> Statuses;

        static CommonConstants()
        {
            Replacments = new Dictionary<string, string>
            {
                { "buy", "BUY" },
                { "make", "MAKE" },
                { "nobom", "NO_BOOM" },

            };

            BomMakePictures = new Dictionary<string, string>
            {
                { "Make", "bom_make.gif" },
                { "bom_make.gif", "bom_make.gif" },
                { "bom_buy.gif", "bom_buy.gif" },
                { "bom_nobom.gif", "bom_nobom.gif" },
            };

            ElementTypePictures = new List<string>
            {
                "ic_sldasm.png",
                "ic_pdf.png",
                "ic_sldprt.png",
                "ic_doc.png",
                "ic_gif.png",
                "ic_zip.png",
                "ic_generic.png",
                "ic_dwg.png"
            };

            Statuses = new List<string>
                {
                    "checkedin.png",
                    "checkedout.png",
                    "state.png",
                    "frozen.png",
                    "released.png",
                    "blank.png"
                };
        }

        public static string GetReplacment(string name)
        {
            var replacment = Replacments.Where(x => name.Contains(x.Key)).FirstOrDefault().Value;

            if (replacment == null)
                return string.Empty;
            return replacment;
        }

        public static string GetMakeBuyReplacment(string content)
        {
            foreach (var pictures in BomMakePictures)
            {
                if (content.Contains(pictures.Key))
                    return pictures.Value;
            }
            return string.Empty;
        }

        public static string GetElementTypePicture(string content)
        {
            foreach (var picture in ElementTypePictures)
            {
                if (content.Contains(picture))
                    return picture;
            }
            return string.Empty;
        }

        public static string GetStatus(string innerHtml)
        {
            foreach (var status in Statuses)
            {
                if (innerHtml.Contains(status))
                    return status;
            }
            return string.Empty; 
        }
    }
}
