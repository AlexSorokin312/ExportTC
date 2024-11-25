using ExportTC.Constants;
using HenconExport.Model.Elemnts;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExportTC.Model
{
    public class ParametersDefinder
    {
        // Словарь для определения расширений файлов и их типов
        private static readonly Dictionary<string, Action<Element, string>> fileTypeActions = new()
        {
            { ".pdf", (e, f) => { e.PDFFile = f; e.TCType = "PDF"; } },
            { ".zip", (e, f) => { e.ZipFile = f; e.TCType = "ZIP"; } },
            { ".sldprt", (e, f) => e.PartFile = f },
            { ".dwg", (e, f) => e.DrawingFile = f },
            { ".doc", (e, f) => e.DocFile = f },
            { ".docx", (e, f) => e.DocxFile = f },
            { ".jpg", (e, f) => e.JpegFile = f }
        };

        public string DefineElementType(Element element)
        {
            return element.DrawingIcon switch
            {
                ElementConstants.DETAIL => ElementConstants.DETAIL,
                ElementConstants.ASSEMBLY => ElementConstants.ASSEMBLY,
                _ => ElementConstants.DRAFT,
            };
        }

        public string DefineRootElementAssembly(Element element)
        {
            return $"{element.Designation}.SLDASM";
        }

        public string DefineSpare(string spare)
        {
            if (string.IsNullOrEmpty(spare)) return string.Empty;

            spare = spare.Replace(';', ' ');

            return spare switch
            {
                var s when s.Contains("N/A") => string.Empty,
                var s when s.Contains("M M") => "M",
                _ => spare,
            };
        }

        public string DefineMakeBuy(string makeBuy)
        {
            return CommonConstants.GetMakeBuyReplacmentText(makeBuy);
        }

        public void DefineFiles(Element element)
        {
            var fileName = element.FileName;
            if (fileName == null)
            {
                element.AssemblyFile = $"{element.Designation}.SLDASM";
                return;
            }

            // Преобразуем имя файла в нижний регистр для унификации
            var fileExtension = Path.GetExtension(fileName)?.ToLower();

            if (fileExtension != null && fileTypeActions.ContainsKey(fileExtension))
            {
                fileTypeActions[fileExtension](element, fileName);
            }
            else
            {
                // Обработка для неизвестных типов файлов
                Console.WriteLine($"Неизвестный файл: {fileName}");
            }
        }
    }
}
