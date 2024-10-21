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
                    Children = new List<Element>() 
                };

                if (elements.Count > 0) //Чтобы корневой элемент не добавлялся 2 раза
                    elements.Add(newElement);

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
            return elements; 
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
                if (line.Contains("<DIV"))
                    outputLines.Add("<DIV>");
                if (line.Contains("</DIV>"))
                    outputLines.Add("</DIV>");

                var matches = Regex.Matches(line, @"<a[^>]*>(\d+)<\/a>");
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                        outputLines.Add(match.Groups[1].Value);
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

                if (trimmedLine.Contains("<DIV") || trimmedLine.Contains("</DIV>"))
                    continue;

                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    outputLines.Add(new string('\t', indentLevel) + trimmedLine);
                }

                var matches = Regex.Matches(trimmedLine, @"<a[^>]*>(\d+)<\/a>");
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        outputLines.Add(new string('\t', indentLevel) + match.Groups[1].Value);
                    }
                }
            }
            return outputLines;
        }
    }
}
