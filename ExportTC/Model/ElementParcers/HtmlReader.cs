using ExportTC.Constants;
using ExportTC.Extensions;
using ExportTC.Interfaces;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls.Ribbon;

namespace ExportTC.Model.ElementParcers
{
    public class HtmlReader : IHtmlReader
    {
        private HtmlAgilityPack.HtmlDocument? _htmlDocument;

        public void FillDataFromHtml(string htmlPath, List<Element> treeElements)
        {
            _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.Load(htmlPath);

            var rows = _htmlDocument.DocumentNode.SelectNodes("//tr");

            if (rows?.Count <= 0) return;

            foreach (var row in rows)
            {
                var cols = row.SelectNodes("td");
                if (cols == null || cols.Count < 5) continue;

                var designation = ExtractDesignation(cols[0].InnerHtml);
                if (designation.Contains("448006350"))
                {

                }
                var type = ExtractImageTypeFromColumn(cols[0].InnerHtml);
                var elementsToUpdate = treeElements.Where(e => e.Designation == designation && string.IsNullOrEmpty(e.Name)).ToList();

                if (elementsToUpdate.Count > 1)
                {
                    if (type == ElementConstants.DETAIL || type == ElementConstants.ASSEMBLY)
                    {
                        var elementNoUpdate = elementsToUpdate.FirstOrDefault(x=>x.Children.Count == 0);
                        elementsToUpdate.Remove(elementNoUpdate);
                    }
                    else
                    {
                        var elementsNoUpdate = elementsToUpdate.Where(x => x.Children.Count > 0).ToList();
                        elementsNoUpdate.ForEach(x => elementsToUpdate.Remove(x));
                    }
                }

                foreach (var elementToUpdate in elementsToUpdate)
                {
                    if (elementToUpdate != null)
                    {
                        elementToUpdate.DrawingIcon = type;
                        elementToUpdate.Quantity = cols[1].InnerText;
                        elementToUpdate.Name = cols[2].InnerText.Clean();
                        elementToUpdate.MakeOrBuy = ExtractMakeOrBuyFromColumn(cols[3].InnerHtml);
                        elementToUpdate.Revision = cols[4].InnerText.Clean();
                        elementToUpdate.FileName = ExtractHrefValueFromColumn(cols[0].InnerHtml, htmlPath);
                        elementToUpdate.ProductStatus = ExtractStatusFromColumn(cols[0].InnerHtml);
                        elementToUpdate.Designation = designation;
                        if (string.IsNullOrEmpty(elementToUpdate.Revision))
                            elementToUpdate.Revision = "00";
                    }
                }
            }
            var m = treeElements.FirstOrDefault(x => x.Designation.Contains("MG BORDER"));
        }

        private string ExtractDesignation(string innerHtml)
        {
            if (innerHtml.Contains("448001671"))
            {

            }
            var match = Regex.Match(innerHtml, @"<a.*?href=""\d+\.htm"".*?>(\d+)<\/a>");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private string ExtractHrefValueFromColumn(string innerHtml, string htmlPath)
        {
            var match = Regex.Match(innerHtml, @"href=""(\d+\.htm)""");
            var result = match.Success ? match.Groups[1].Value : null;

            if (result == null)
                return null;

            string directory = Path.GetDirectoryName(htmlPath);

            string foundFilePath = FindFileInSubdirectories(directory, result);
            string extractedFileName = FileNameExtactor.ExtractFileNameFromText(foundFilePath);
            return extractedFileName;
        }

        private string FindFileInSubdirectories(string directory, string fileName)
        {
            try
            {
                var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
                return files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске файла: {ex.Message}");
                return null;
            }
        }

        private static string ExtractStatusFromColumn(string innerHtml)
            => CommonConstants.GetStatus(innerHtml);

        private string ExtractMakeOrBuyFromColumn(string innerHtml)
            => CommonConstants.GetMakeBuyReplacmentImage(innerHtml);

        private string ExtractImageTypeFromColumn(string innerHtml)
            => CommonConstants.GetElementTypePicture(innerHtml);

        private static string ExtractDrawingImageTypeFromColumn(string innerHtml)
        {
            if (innerHtml.Contains("ic_sw_drw2.png"))
                return "ic_sw_drw2.png";
            if (innerHtml.Contains("ic_inv_drw2.png"))
                return "ic_inv_drw2.png";

            return "-";
        }
    }
}
