using ExportTC.Interfaces;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Text.RegularExpressions;

namespace ExportTC.Model.ElementParcers
{
    public class ElementTreeBuilder : IElementTreeBuilder
    {
        public List<Element> BuildTreeWithParents(string htmlPath)
        {
            var elements = new List<Element>();
            var stack = new Stack<Element>();
            var rowsWithIndent = FormIndentForStructure(htmlPath);

            foreach (var line in rowsWithIndent)
            {
                int indentLevel = GetIndentLevel(line);
                var newElement = new Element
                {
                    Designation = line.Trim(),
                    Children = new()
                };

                while (stack.Count > indentLevel)
                {
                    stack.Pop();
                }

                if (stack.Count > 0)
                {
                    var parent = stack.Peek();
                    parent.Children.Add(newElement);
                    newElement.Parent = parent;
                }
                else
                {
                    elements.Add(newElement);
                }

                stack.Push(newElement);
            }

            // Ищем ветку, содержащую "General"
            var generalBranch = FindGeneralElement(elements);
            return new List<Element> { generalBranch};

            return elements;
        }

        // Метод для поиска первого элемента, содержащего "General" в Designation
        private Element FindGeneralElement(List<Element> elements)
        {
            foreach (var element in elements)
            {
                // Проверяем, содержит ли Designation слово "General"
                if (element.Designation.Contains("General", StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }

                // Рекурсивно проверяем дочерние элементы
                var child = FindGeneralElement(element.Children);
                if (child != null) // Если поддерево найдено
                {
                    return child;
                }
            }

            // Если не нашли "General", возвращаем null
            return null;
        }

        private static int GetIndentLevel(string line)
        {
            int level = 0;
            foreach (char c in line)
            {
                if (c == '\t') level++;
                else break;
            }
            return level;
        }

        private List<string> GetInitialStructure(string inputFilePath)
        {
            var lines = File.ReadAllLines(inputFilePath);
            var outputLines = new List<string>();

            foreach (var line in lines)
            {
                // Пропускаем строки, содержащие нежелательные теги
                if (Regex.IsMatch(line, @"<\s*IMG[^>]*>", RegexOptions.IgnoreCase) ||
                    line.Contains("BORDER=", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Пропускаем "кривые" строки
                }

                // Ищем открывающие теги <DIV>
                if (Regex.IsMatch(line, @"<\s*DIV[^>]*>", RegexOptions.IgnoreCase))
                    outputLines.Add("<DIV>");

                // Ищем закрывающие теги </DIV>
                if (Regex.IsMatch(line, @"<\s*/\s*DIV\s*>", RegexOptions.IgnoreCase))
                    outputLines.Add("</DIV>");

                // Ищем содержимое внутри тегов <a>
                var matches = Regex.Matches(line, @"<a[^>]*>(.*?)<\/a>", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                        outputLines.Add(match.Groups[1].Value.Trim());
                }
            }

            return outputLines;
        }


        private List<string> FormIndentForStructure(string path)
        {
            var lines = GetInitialStructure(path);
            var outputLines = new List<string>();
            int indentLevel = 0;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.Equals("<DIV>", StringComparison.OrdinalIgnoreCase))
                {
                    indentLevel++;
                    continue;
                }
                if (trimmedLine.Equals("</DIV>", StringComparison.OrdinalIgnoreCase))
                {
                    indentLevel--;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    outputLines.Add(new string('\t', indentLevel) + trimmedLine);
                }
            }

            return outputLines;
        }

        public List<Element> FlattenTree(List<Element> treeElements)
        {
            var flatList = new List<Element>();
            foreach (var element in treeElements)
            {
                FlattenElement(element, flatList);
            }
            return flatList;
        }

        private void FlattenElement(Element element, List<Element> flatList)
        {

            flatList.Add(element);
            foreach (var child in element.Children)
            {
                FlattenElement(child, flatList);
            }
        }
    }
}
