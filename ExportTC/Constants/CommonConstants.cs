namespace ExportTC.Constants
{
    public static class CommonConstants
    {
        public const string DirectoryDialogDescription = "Выберите папку";
        public const string ExcelFileDialogFilter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";
        public const string HtmFileDialogFilter = "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
        public const string DefaultResultFileName = "Hencon_Imp.xlsm";
        
        public static Dictionary<string, string> Replacments;

        public static Dictionary<string, string> BomMakePictures;

        public static Dictionary<string, string> ElementTypePictures;
        public static Dictionary<string, string> Statuses;



        static CommonConstants()
        {
            Replacments = new Dictionary<string, string>
            {
                { "BUY", "BUY" },
                { "MAKE", "MAKE" },

            };

            BomMakePictures = new Dictionary<string, string>
            {
                { "bom_make.gif", "MAKE" },
                { "bom_buy.gif", "BUY" },
                { "bom_nobom.gif", string.Empty },
            };

            ElementTypePictures = new Dictionary<string, string>
            {
                { "ic_sldasm.png", ElementConstants.ASSEMBLY},
                { "ic_sldprt.png", ElementConstants.DETAIL},
                { "ic_pdf.png", ElementConstants.PDF},
                { "ic_zip.png", ElementConstants.ZIP},
                { "ic_doc.png", ElementConstants.DOC},
                { "ic_gif.png", ElementConstants.GIF},
                { "ic_generic.png", ElementConstants.GENERIC},

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

        public static string GetMakeBuyReplacmentImage(string content)
        {
            foreach (var pictures in BomMakePictures)
            {
                if (content.Contains(pictures.Key))
                    return pictures.Value;
            }
            return string.Empty;
        }

        public static string GetMakeBuyReplacmentText(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            foreach (var pictures in Replacments)
            {
                if (content.Contains(pictures.Key))
                    return pictures.Value;
            }
            return string.Empty;
        }

        public static string GetElementTypePicture(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
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
