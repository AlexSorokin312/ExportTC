using ExportTC.Constants;
using ExportTC.Interfaces;
using HenconExport;
using HenconExport.Model.Elemnts;
using System.Text.RegularExpressions;

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

                // Найти элемент в treeElements с совпадающим Designation
                var elementToUpdate = treeElements.FirstOrDefault(e => e.Designation == designation);
                if (elementToUpdate != null)
                {
                    // Обновить данные существующего элемента
                    elementToUpdate.Quantity = cols[1].InnerText;
                    elementToUpdate.Name = cols[2].InnerText.Clean();
                    elementToUpdate.MakeOrBuy = ExtractMakeOrBuyFromColumn(cols[3].InnerHtml);
                    elementToUpdate.Revision = cols[4].InnerText.Clean();
                    elementToUpdate.FileName = ExtractHrefValueFromColumn(cols[0].InnerHtml);
                    elementToUpdate.ProductStatus = ExtractStatusFromColumn(cols[0].InnerHtml);
                    elementToUpdate.Type = ExtractImageTypeFromColumn(cols[0].InnerHtml);
                    elementToUpdate.Drawing = ExtractDrawingImageTypeFromColumn(cols[0].InnerHtml);
                }
            }
        }

        private string ExtractDesignation(string innerHtml)
        {
            var match = Regex.Match(innerHtml, @"<a.*?href=""\d+\.htm"".*?>(\d+)<\/a>");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private string ExtractHrefValueFromColumn(string innerHtml)
        {
            var match = Regex.Match(innerHtml, @"href=""(\d+\.htm)""");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string ExtractStatusFromColumn(string innerHtml)
            => CommonConstants.GetStatus(innerHtml);

        private string ExtractMakeOrBuyFromColumn(string innerHtml)
            => CommonConstants.GetMakeBuyReplacment(innerHtml);

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
