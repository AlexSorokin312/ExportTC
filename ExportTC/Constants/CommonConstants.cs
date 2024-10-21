using System.Windows.Input;

namespace ExportTC.Constants
{
    public static class CommonConstants
    {
        public const string DirectoryDialogDescription = "Выберите папку";
        public const string ExcelFileDialogFilter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";
        public const string HtmFileDialogFilter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";

        public static Dictionary<string, string> Replacments;

        public static Dictionary<string, string> BomMakePictures;

        public static Dictionary<string, string> ElementTypePictures;
        public static Dictionary<string, string> Statuses;

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
                { "bom_make.gif", "Make" },
                { "bom_buy.gif", "Buy" },
                { "bom_nobom.gif", string.Empty },
            };

            ElementTypePictures = new Dictionary<string, string>
            {
                { "ic_sldasm.png", ElementConstants.ASSEMBLY},
                { "ic_pdf.png", ElementConstants.PDF },
                { "ic_sldprt.png", ElementConstants.DETAIL},
                { "ic_doc.png", ElementConstants.DOC },
                { "ic_gif.png", ElementConstants.GIF },
                { "ic_zip.png", ElementConstants.ZIP },
                { "ic_generic.png", ElementConstants.GENERIC },
                { "ic_dwg.png", ElementConstants.DWG }
            };

            Statuses = new Dictionary<string, string>
                {
                    {"checkedin.png", ElementConstants.CHECKEDIN},
                    {"checkedout.png", ElementConstants.CHECKEDOUT},
                    {"state.png", ElementConstants.STATE},
                    {"frozen.png", ElementConstants.FROZEN},
                    {"released.png", ElementConstants.RELEASED},
                    {"blank.png", ElementConstants.BLANK},
                    {"new.png", ElementConstants.NEW},
                    {"new.gif", ElementConstants.NEW},
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
                if (content.Contains(picture.Key))
                    return picture.Value;
            }
            return string.Empty;
        }

        public static string GetStatus(string innerHtml)
        {
            foreach (var status in Statuses)
            {
                if (innerHtml.Contains(status.Key))
                    return status.Value;
            }
            return string.Empty; 
        }
    }
}
