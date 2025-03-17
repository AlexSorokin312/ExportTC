using ExportTC.Constants;
using ExportTC.Extensions;
using ExportTC.Interfaces;
using HenconExport.Model.Elemnts;

namespace ExportTC.Model.ElementParcers
{
    public class HtmlReader : IHtmlReader
    {
        private HtmlAgilityPack.HtmlDocument? _htmlDocument;

        public void FillDataFromHtml(string htmlPath, List<Element> treeElements)
        {
            // Создание словаря для быстрого доступа
            var elementsDict = treeElements
                .Where(e => string.IsNullOrEmpty(e.Name)).GroupBy(e => e.Designation).ToDictionary(g => g.Key, g => g.ToList());

            // Загрузка HTML-документа
            _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.Load(htmlPath);

            var rows = _htmlDocument.DocumentNode.SelectNodes("//tr");

            if (rows?.Count <= 0)
                return;

            foreach (var row in rows)
            {
                ProcessRow(row, elementsDict, htmlPath);
            }
        }

        private Dictionary<string, int> encounters = new();

        private void ProcessRow(HtmlAgilityPack.HtmlNode row, Dictionary<string, List<Element>> elementsDict, string htmlPath)
        {
            var cols = row.SelectNodes("td");
            if (cols == null || cols.Count < 5)
                return;

            var designation = FileNameExtactor.ExtractDesignation(cols[0].InnerHtml);
            var type = ExtractImageTypeFromColumn(cols[0].InnerHtml);

            // Проверяем, есть ли данные в словаре
            if (elementsDict.TryGetValue(designation, out var elementsToUpdate) && elementsToUpdate != null)
            {
                    if (type == "Pdf" || type == "Zip" || type == "Doc" || type == "Gif")
                {
                        elementsToUpdate.ForEach(element =>
                        {
                            if (element.Children?.Count != 0)
                            {
                                element.FileName = FileNameExtactor.ExtractHrefValueFromColumn(cols[0].InnerHtml, htmlPath);
                            }

                        });
                }
                // Копия для работы в потоке
                var elementsLocalCopy = new List<Element>(elementsToUpdate);

                if (elementsLocalCopy.Count > 1)
                {
                    if (type == ElementConstants.DETAIL || type == ElementConstants.ASSEMBLY || type == ElementConstants.GENERIC)
                    {
                        var elementNoUpdate = elementsLocalCopy.FirstOrDefault(x => x.Children.Count == 0);

                        var counts = encounters.FirstOrDefault(x => x.Key == designation).Value;
                        var c = elementsToUpdate.Skip(counts);
                        // Обновление элементов
                        foreach (var elementToUpdate in c)
                        {
                            if (elementToUpdate != null)
                            {
                                UpdateElement(elementToUpdate, cols, type, designation, htmlPath);
                            }
                        }
                        //if (elementNoUpdate != null && type != ElementConstants.GENERIC)
                        //elementsLocalCopy.Remove(elementNoUpdate);
                    }
                    else
                    {
                        var elementsNoUpdate = elementsLocalCopy.Where(x => x?.Children?.Count > 0).ToList();
                        elementsNoUpdate.ForEach(x => elementsLocalCopy.Remove(x));
                        // Обновление элементов
                        foreach (var elementToUpdate in elementsLocalCopy)
                        {
                            if (elementToUpdate != null)
                            {
                                UpdateElement(elementToUpdate, cols, type, designation, htmlPath);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var elementToUpdate in elementsLocalCopy)
                    {
                        if (elementToUpdate != null)
                        {
                            UpdateElement(elementToUpdate, cols, type, designation, htmlPath);
                        }
                    }
                }
            }

            var en = encounters.FirstOrDefault(x => x.Key == designation);
            if (en.Key == null)
                encounters.Add(designation, 1);
            else
                encounters[designation]++;
        }

        private void UpdateElement(Element element, HtmlAgilityPack.HtmlNodeCollection cols, string type, string designation, string htmlPath)
        {
            element.DrawingIcon = type;
            element.Quantity = cols[1].InnerText;
            element.Name = cols[2].InnerText.Clean();
            element.MakeOrBuy = ExtractMakeOrBuyFromColumn(cols[3].InnerHtml);
            element.Revision = cols[4].InnerText.Clean() ?? "00";
            if (string.IsNullOrEmpty(element.Revision))
                element.Revision = "00";
            element.FileName = FileNameExtactor.ExtractHrefValueFromColumn(cols[0].InnerHtml, htmlPath);
            element.ProductStatus = ExtractStatusFromColumn(cols[0].InnerHtml);
            element.Designation = designation;
        }

        private static string ExtractStatusFromColumn(string innerHtml)
            => CommonConstants.GetStatus(innerHtml);

        private string ExtractMakeOrBuyFromColumn(string innerHtml)
            => CommonConstants.GetMakeBuyReplacmentImage(innerHtml);

        private string ExtractImageTypeFromColumn(string innerHtml)
            => CommonConstants.GetElementTypePicture(innerHtml);

    }
}
