using ExportTC.Constants;
using ExportTC.Interfaces;
using HenconExport.Model.Elemnts;

namespace ExportTC.Model.ElementParcers
{
    public class HtmlElementParser
    {
        private IHtmlReader _reader;
        private IElementTreeBuilder _treeBuilder;

        public HtmlElementParser(IHtmlReader reader, IElementTreeBuilder treeBuilder)
        {
            _reader = reader;
            _treeBuilder = treeBuilder;
        }

        public List<Element> GetElementsFromHTML(string htmlPath)
        {
            var allElements = _treeBuilder.BuildTreeWithParents(htmlPath);
            var elements = _treeBuilder.FlattenTree(allElements);


            allElements.FirstOrDefault().Root = true;
            _reader.FillDataFromHtml(htmlPath, elements);

            var list = GetSuspiciousMessages(elements);
            Warnings.warnings = list;
            return elements;
        }


        private void UpdateElementData(Element treeElement, List<Element> allElements)
        {
            foreach (var matchingElement in allElements)
            {
                if (matchingElement.Designation == treeElement.Designation)
                {
                    matchingElement.Parent = treeElement.Parent;
                }
            }

            foreach (var child in treeElement.Children)
            {
                UpdateElementData(child, allElements);
            }
        }

        public List<string> GetSuspiciousMessages(IEnumerable<Element> elements)
        {
            var messages = new List<string>();

            var fileFormats = new List<string>
                {
                    ".pdf", "zip", "rar", "7z", "xl", "doc", "docx", "ppt", "pptx", "msg", "dwg",
                    "jpg", "png", "gif", "bmp", "tif", "txt", "htm", "dxf", "stp", ".E60", ".ccd", ".#PA"
                };

            var assyInDataSet = elements.Where(x =>
                x.DrawingIcon == ElementConstants.DETAIL &&
                x.Parent.FileName != null &&
                fileFormats.Any(format => x.Parent.FileName.Contains(format, StringComparison.OrdinalIgnoreCase))
            );

            foreach (var assy in assyInDataSet)
            {
                messages.Add($"Предупреждение: Деталь/Сборка {assy.Designation} входит в набор данных {assy.Parent.Designation}");
            }


            foreach (var el in elements)
            {

                // Если отсутствует информация о файлах или родительском элементе — пропускаем элемент.
                if (el == null || el.Parent == null ||
                    string.IsNullOrEmpty(el.FileName) ||
                    string.IsNullOrEmpty(el.Parent.FileName) ||
                    el.Parent.DrawingIcon == ElementConstants.DETAIL
                    )
                {
                    continue;
                }

                var children = el.Parent.Children;
                var equalDesignation = children.Where(x => x.Designation == el.Parent.Designation);
                if (equalDesignation.Count() != 0)
                {
                    continue;
                }

                // Определяем формат для элемента и его родителя
                string childFormat = GetFileFormat(el.FileName);
                string parentFormat = GetFileFormat(el.Parent.FileName);

                // Если оба формата определились и они различаются, добавляем сообщение
                if (!string.IsNullOrEmpty(childFormat) && !string.IsNullOrEmpty(parentFormat) &&
                    !childFormat.Equals(parentFormat, StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add($"Предупреждение: элемент '{el.Designation}' имеет формат '{childFormat}', а его родитель '{el.Parent.Designation}' имеет формат '{parentFormat}'.");
                }
            }

            var assyInDetails = elements.Where(x => x.DrawingIcon == ElementConstants.ASSEMBLY && x.Parent.DrawingIcon == ElementConstants.DETAIL);
            foreach (var assy in assyInDetails)
            {
                messages.Add($"Предупреждение: Сборка {assy.Designation} входит в деталь {assy.Parent.Designation}");
            }


            return messages;
        }

        /// <summary>
        /// Определяет формат файла по его имени. Если имя файла содержит подстроку, соответствующую одному из известных форматов, возвращается название формата.
        /// Можно расширять и менять логику при необходимости.
        /// </summary>
        private string GetFileFormat(string fileName)
        {
            // Приводим имя к нижнему регистру для упрощения проверок
            var lowerName = fileName.ToLowerInvariant();

            // Проверяем форматы в порядке приоритета (если могут пересекаться, порядок важен)
            if (lowerName.Contains(".pdf"))
                return "pdf";
            if (lowerName.Contains("xl"))
                return "excel";
            // Объединяем zip и rar в один формат
            if (lowerName.Contains("zip") || lowerName.Contains("rar"))
                return "zip";
            if (lowerName.Contains("jpg"))
                return "jpeg";
            if (lowerName.Contains("png"))
                return "png";
            if (lowerName.Contains("htm"))
                return "html";
            if (lowerName.Contains("7z"))
                return "7z";
            if (lowerName.Contains("gif"))
                return "gif";
            if (lowerName.Contains("dxf"))
                return "dxf";
            if (lowerName.Contains("stp"))
                return "stp";
            if (lowerName.Contains("pptx"))
                return "pptx";
            if (lowerName.Contains("ppt"))
                return "ppt";
            if (lowerName.Contains("tif"))
                return "tif";
            if (lowerName.Contains("txt"))
                return "txt";
            if (lowerName.Contains("bmp"))
                return "bmp";
            if (lowerName.Contains("msg"))
                return "msg";
            if (lowerName.Contains("dwg"))
                return "dwg";
            // "Multy" форматы
            if (lowerName.Contains(".e60") || lowerName.Contains(".ccd") || lowerName.Contains(".#pa"))
                return "multy";
            // Документные файлы: если в имени встречается docx, то считаем, что это docx, иначе если doc — doc
            if (lowerName.Contains("doc"))
                return lowerName.Contains("docx") ? "docx" : "doc";

            // Если не удалось определить формат — возвращаем пустую строку
            return string.Empty;
        }

    }
}

public static class Warnings
{
    public static List<string> warnings = new();
}

