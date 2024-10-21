using ExportTC;
using ExportTC.Constants;
using ExportTC.Model;
using HenconExport.Model.Elemnts;
using System.IO;
using System.Text.RegularExpressions;

namespace HenconExport.Model.HTMHandler
{
    public class HtmlHandler : IHtmlHadnler
    {
        private string outputFilePath = @"C:\Users\ASorokin\Downloads\Telegram Desktop\For Anton\For Anton\output.txt";
        private string excelOutPut = @"C:\Users\ASorokin\Downloads\Telegram Desktop\For Anton\For Anton\result.xlsx";

        private HtmlAgilityPack.HtmlDocument? _htmlDocument;

        private InitialData _initialData;

        public HtmlHandler(string path)
        {
            _htmlDocument = new HtmlAgilityPack.HtmlDocument();
            _htmlDocument.Load(path);
        }

        public List<TreeElementHTML> GetElementsFromHTML(InitialData initialData)
        {
            _initialData = initialData;
            List<TreeElementHTML> elements = new List<TreeElementHTML>();

            var rowsWithIndent = FormIndentForStructure(_initialData.HtmlFile);
            var tree = BuildTreeWithParents(rowsWithIndent);

            var allElements = ParseHtmlToTreeElements(_initialData.HtmlFile);
            UpdateTreeWithData(tree, allElements);

            return allElements;
        }

        static void UpdateTreeWithData(List<TreeElementHTML> tree, List<TreeElementHTML> allElements)
        {
            foreach (var element in tree)
            {
                UpdateElementData(element, allElements); 
            }
        }


        static void UpdateElementData(TreeElementHTML treeElement, List<TreeElementHTML> allElements)
        {
            foreach (var matchingElement in allElements)
            {
                if (matchingElement.Designation == treeElement.Designation)
                {
                    // Обновляем недостающие поля
                    treeElement.Quantity = matchingElement.Quantity;
                    treeElement.Name = matchingElement.Name;
                    treeElement.MakeOrBuy = matchingElement.MakeOrBuy;
                    treeElement.Revision = matchingElement.Revision;
                    treeElement.Status = matchingElement.Status;
                    treeElement.Type = matchingElement.Type;
                    treeElement.Drawing = matchingElement.Drawing;
                    treeElement.FileName = matchingElement.FileName;
                    matchingElement.Parent = treeElement.Parent;
                }
            }

            foreach (var child in treeElement?.Children)
            {
                UpdateElementData(child, allElements);
            }
        }

        static List<TreeElementHTML> BuildTreeWithParents(List<string> lines)
        {
            List<TreeElementHTML> tree = new List<TreeElementHTML>();
            Stack<TreeElementHTML> stack = new Stack<TreeElementHTML>();

            foreach (var line in lines)
            {
                int indentLevel = GetIndentLevel(line);
                string value = line.Trim(); 

                TreeElementHTML newElement = new TreeElementHTML() { Designation = value };

                if (stack.Count == 0)
                    tree.Add(newElement);
                else
                {
                    while (stack.Count > indentLevel)
                    {
                        stack.Pop();
                    }

                    if (stack.Count > 0)
                    {
                        TreeElementHTML parent = stack.Peek();
                        parent?.Children?.Add(newElement);
                        newElement.Parent = parent;
                    }
                    else
                        tree.Add(newElement);
                }

                stack.Push(newElement);
            }
            return tree;
        }

        static int GetIndentLevel(string line)
        {
            int level = 0;
            foreach (char c in line)
            {
                if (c == '\t') level++;
                else break;
            }
            return level;
        }


        private List<string> FormIndentForStructure(string path)
        {
            var lines = GetInitialStructure(path);
            var outputLines = new List<string>();
            int indentLevel = 0; 

            for (int i = 0; i < lines.ToArray().Length; i++)
            {
                var line = lines[i].Trim();

                if (line.Equals("<DIV>", StringComparison.OrdinalIgnoreCase))
                {
                    indentLevel++;
                    continue; 
                }
                if (line.Equals("</DIV>", StringComparison.OrdinalIgnoreCase))
                {
                    indentLevel--; 
                    continue; 
                }

                if (line.Contains("<DIV") || line.Contains("</DIV>"))
                    continue; 

                if (!string.IsNullOrWhiteSpace(line))
                {
                    outputLines.Add(new string('\t', indentLevel) + line);
                }

                var matches = Regex.Matches(line, @"<a[^>]*>(\d+)<\/a>");
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

        private List<TreeElementHTML> ParseHtmlToTreeElements(string filePath)
        {
            List<TreeElementHTML> treeElements = new List<TreeElementHTML>();

            var rows = _htmlDocument?.DocumentNode.SelectNodes("//tr");
            if (rows?.Count <= 0)
                return treeElements;

            foreach (var row in rows)
            {
                var cols = row.SelectNodes("td");

                if (cols == null || cols.Count < 5)
                    continue;

                var htmlString = cols[0].InnerHtml;
                var description = cols[2].InnerText.Clean();
                var quantity = cols[1].InnerText;
                var makeOrBuy = ExtractMakeOrBuyFromColumn(cols[3].InnerHtml);
                var revision = cols[4].InnerText.Clean();
                var htmlFileName = ExtractHrefValueFromColumn(cols[0].InnerHtml);

                //Извлекаем файл
                //string baseDirectory = Constants.BASE_HTML_DIRECTORY;
                //string fullPath = Path.Combine(baseDirectory, htmlFileName);
                //string extractedFileName = FileNameExtactor.ExtractFileNameFromText(fullPath);

                var designation = ExtractDesignation(htmlString);

                var status = ExtractStatusFromColumn(cols[0].InnerHtml);
                var type = ExtractImageTypeFromColumn(cols[0].InnerHtml);

                var secondary = ExtractSecondaryImageTypeFromColumn(cols[0].InnerHtml);

                TreeElementHTML element = new TreeElementHTML
                {
                    Designation = designation,
                    Quantity = quantity,
                    Name = description,
                    MakeOrBuy = makeOrBuy,
                    Revision = revision,
                    Status = status,
                    Type = type,
                    Drawing = secondary,
                   // FileName = extractedFileName
                };

                treeElements.Add(element);
            }
            return treeElements;
        }

        private string ExtractDesignation(string innerHtml)
        {
            var match = Regex.Match(innerHtml, @"<a.*?href=""\d+\.htm"".*?>(\d+)<\/a>");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private string ExtractHrefValueFromColumn(string innerHtml)
        {
            var match = Regex.Match(innerHtml, @"href=""(\d+\.htm)""");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        static string ExtractStatusFromColumn(string innerHtml)
        {

            if (innerHtml.Contains("checkedin.png"))
                return "checkedin.png";
            if (innerHtml.Contains("checkedout.png"))
                return "checkedout.png";
            if (innerHtml.Contains("state.png"))
                return "state.png";
            if (innerHtml.Contains("frozen.png"))
                return "frozen.png";
            if (innerHtml.Contains("released.png"))
                return "released.png";
            if (innerHtml.Contains("blank.png"))
                return "blank.png";


            return "Unknown";
        }


        private string ExtractMakeOrBuyFromColumn(string innerHtml) =>
            Constants.GetMakeBuyReplacment(innerHtml);


        private string ExtractImageTypeFromColumn(string innerHtml)=>
            Constants.GetElementTypePicture(innerHtml);


        static string ExtractSecondaryImageTypeFromColumn(string innerHtml)
        {
            if (innerHtml.Contains("ic_sw_drw2.png"))
                return "ic_sw_drw2.png";
            if (innerHtml.Contains("ic_inv_drw2.png"))
                return "ic_inv_drw2.png";

            // Если изображение не найдено
            return "-";
        }

        private List<string> GetInitialStructure(string inputFilePath)
        {
            {
                string[] lines = File.ReadAllLines(inputFilePath);
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
        }
    }
}
