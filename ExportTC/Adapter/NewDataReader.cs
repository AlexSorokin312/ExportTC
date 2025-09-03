
using ExportTC.Constants;
using HenconExport;
using HenconExport.Model.Elemnts;
using MigrateData.Models; // ваш ExcelReader

namespace MigrateData.Adapter
{
    internal class NewDataReader
    {
        // Ожидаемые заголовки (можно расширять)
        private static readonly string[] ExpectedHeaders = new[]
        {
            "HierarchyPath",
            "UNIQUE_ID",
            "ID",
            "DESCRIPTION",
            "MAKE_BUY",
            "COSTTYPE",
            "SPARES",
            "REVISION",
            "ITEM_CODE_SUPPLIER",
            "ADD_INFO",
            "HENCON_STD",
            "Level",
            "Type",
            "FILE_NAME",
            "BOM",
            "LONGPATH",
            "TQ"
        };

        /// <summary>
        /// Читает таблицу Excel и возвращает коллекцию NewDataItem.
        /// Метод ищет заголовки в строке headerRow (по умолчанию 1) и затем читает строки данных.
        /// </summary>
        public List<NewDataItem> ReadAllColumns(string filePath, int sheetIndex = 0, int headerRow = 1, int maxColumnsToScan = 80)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(nameof(filePath));
            if (sheetIndex < 0) throw new ArgumentOutOfRangeException(nameof(sheetIndex));
            if (headerRow < 1) throw new ArgumentOutOfRangeException(nameof(headerRow));
            if (maxColumnsToScan < 1) maxColumnsToScan = 80;

            using (var reader = new ExcelReader(filePath))
            {
                // header -> columnName (A, B, ...)
                var headerToColumn = MapHeaders(reader, sheetIndex, headerRow, maxColumnsToScan);

                if (headerToColumn.Count == 0)
                    return new List<NewDataItem>();

                // primary column для поиска последней строки: HierarchyPath, иначе первая найденная
                string primaryColumn = headerToColumn.ContainsKey("HierarchyPath")
                    ? headerToColumn["HierarchyPath"]
                    : headerToColumn.Values.First();

                int startRow = headerRow + 1;
                int lastRow = reader.GetLastUsedRow(sheetIndex, primaryColumn, startRow);
                if (lastRow < startRow)
                    return new List<NewDataItem>();

                var result = new List<NewDataItem>(lastRow - startRow + 1);

                for (int row = startRow; row <= lastRow; row++)
                {
                    var item = new NewDataItem
                    {
                        ID = ReadMappedCell(reader, sheetIndex, headerToColumn, "ID", row),
                        DESCRIPTION = ReadMappedCell(reader, sheetIndex, headerToColumn, "DESCRIPTION", row),
                        MAKE_BUY = ReadMappedCell(reader, sheetIndex, headerToColumn, "MAKE_BUY", row),
                        COSTTYPE = ReadMappedCell(reader, sheetIndex, headerToColumn, "COSTTYPE", row),
                        SPARES = ReadMappedCell(reader, sheetIndex, headerToColumn, "SPARES", row),
                        REVISION = ReadMappedCell(reader, sheetIndex, headerToColumn, "REVISION", row),
                        ITEM_CODE_SUPPLIER = ReadMappedCell(reader, sheetIndex, headerToColumn, "ITEM_CODE_SUPPLIER", row),
                        ADD_INFO = ReadMappedCell(reader, sheetIndex, headerToColumn, "ADD_INFO", row),
                        HENCON_STD = ReadMappedCell(reader, sheetIndex, headerToColumn, "HENCON_STD", row),
                        Level = ReadMappedCell(reader, sheetIndex, headerToColumn, "Level", row),
                        Type = ReadMappedCell(reader, sheetIndex, headerToColumn, "Type", row),
                        FILE_NAME = ReadMappedCell(reader, sheetIndex, headerToColumn, "FILE_NAME", row),
                        BOM = ReadMappedCell(reader, sheetIndex, headerToColumn, "BOM", row),
                        LONG_PATH = ReadMappedCell(reader, sheetIndex, headerToColumn, "LONGPATH", row),
                        Quantity = ReadMappedCell(reader, sheetIndex, headerToColumn, "TQ", row)
                    };

                    result.Add(item);
                }
                return result;
            }
        }

        // Сопоставляет найденные заголовки (в headerRow) и возвращает словарь expectedHeader -> columnName (A..Z..)
        private Dictionary<string, string> MapHeaders(ExcelReader reader, int sheetIndex, int headerRow, int maxColumnsToScan)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var normalizedExpected = ExpectedHeaders.ToDictionary(h => NormalizeHeader(h), h => h);

            for (int colIndex = 1; colIndex <= maxColumnsToScan; colIndex++)
            {
                var colName = ToExcelColumnName(colIndex);
                var headerValue = reader.ReadCell(sheetIndex, colName, headerRow);
                if (string.IsNullOrWhiteSpace(headerValue))
                    continue;

                var norm = NormalizeHeader(headerValue);
                if (normalizedExpected.TryGetValue(norm, out var expectedOriginal))
                {
                    if (!map.ContainsKey(expectedOriginal))
                        map[expectedOriginal] = colName;
                }

                // Попробуем более гибкое совпадение: если headerValue содержит ожидаемый (без пробелов/подчёркиваний)
                if (!map.ContainsKey("HierarchyPath"))
                {
                    // пример: "Hierarchy Path" -> normalize-> "hierarchypath"
                    foreach (var kv in normalizedExpected)
                    {
                        if (norm.Contains(kv.Key)) // если название заголовка содержит ожидаемую нормализованную форму
                        {
                            if (!map.ContainsKey(kv.Value))
                                map[kv.Value] = colName;
                        }
                    }
                }

                if (map.Count == ExpectedHeaders.Length)
                    break;
            }

            return map;
        }

        private static string NormalizeHeader(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var cleaned = s.Trim().Replace(" ", "").Replace("_", "").Replace("-", "");
            return cleaned.ToLowerInvariant();
        }

        private static string ReadMappedCell(ExcelReader reader, int sheetIndex, Dictionary<string, string> headerToColumn, string headerKey, int row)
        {
            if (headerToColumn.TryGetValue(headerKey, out var col))
            {
                try
                {
                    return reader.ReadCell(sheetIndex, col, row) ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        // 1 -> A, 27 -> AA
        private static string ToExcelColumnName(int columnNumber)
        {
            if (columnNumber < 1) throw new ArgumentOutOfRangeException(nameof(columnNumber));
            var dividend = columnNumber;
            var colName = string.Empty;
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                colName = Convert.ToChar('A' + modulo) + colName;
                dividend = (dividend - modulo) / 26;
            }
            return colName;
        }

        public List<NewDataItem> BuildTreeFromLongPaths(List<NewDataItem> flatItems)
        {
            if (flatItems == null) throw new ArgumentNullException(nameof(flatItems));

            // Словарь id -> node (из исходного списка)
            var dict = new Dictionary<string, NewDataItem>(StringComparer.OrdinalIgnoreCase);

            // Первично добавим все исходные элементы (если ID пустой — пропускаем)
            foreach (var it in flatItems)
            {
                if (string.IsNullOrWhiteSpace(it.ID)) continue;
                if (!dict.ContainsKey(it.ID))
                {
                    // Убедимся, что Children не null
                    if (it.Children == null)
                        it.Children = new List<NewDataItem>();
                    dict[it.ID] = it;
                }
            }

            // Для каждого элемента разберём LONG_PATH и соединим пары parent->child
            foreach (var original in flatItems)
            {
                if (string.IsNullOrWhiteSpace(original.LONG_PATH)) continue;

                var pathIds = ParsePath(original.LONG_PATH).ToList();
                if (pathIds.Count == 0) continue;

                // Проходим по путю по парам (path[i-1] -> path[i])
                for (int i = 0; i < pathIds.Count; i++)
                {
                    var id = pathIds[i];
                    var node = GetOrCreateNode(dict, id);

                    // Если есть предок в пути, свяжем
                    if (i > 0)
                    {
                        var parentId = pathIds[i - 1];
                        var parent = GetOrCreateNode(dict, parentId);

                        // Установим parent (если ещё не установлен) и добавим в children
                        if (node != null && parent != null)
                        {
                            // установим Parent (перезаписывать не будем, если уже есть)
                            if (node.Parent == null)
                                node.Parent = parent;

                            // убедимся, что у parent есть список детей
                            if (parent.Children == null)
                                parent.Children = new List<NewDataItem>();

                            // добавим child, если ещё нет
                            if (!parent.Children.Any(c => string.Equals(c.ID, node.ID, StringComparison.OrdinalIgnoreCase)))
                                parent.Children.Add(node);
                        }
                    }
                }
            }

            // Проставим Root у тех, у кого Parent == null; у остальных — false
            foreach (var kv in dict.Values)
            {
                kv.Root = kv.Parent == null;
                if (kv.Children == null)
                    kv.Children = new List<NewDataItem>();
            }

            // Вернём корни (часто один корень, но метод универсален)
            return dict.Values.Where(x => x.Parent == null).OrderBy(x => x.ID).ToList();
        }

        /// <summary>
        /// Разбирает строку LONG_PATH и возвращает перечисление id в порядке от корня к листу.
        /// Примеры входа:
        /// "447020419" -> ["447020419"]
        /// "447020419 -> 447020797 -> 447020415" -> ["447020419","447020797","447020415"]
        /// </summary>
        private static IEnumerable<string> ParsePath(string longPath)
        {
            if (string.IsNullOrWhiteSpace(longPath))
                yield break;

            // Разделители: "->" или просто пробелы вокруг стрелки
            var parts = longPath.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var id = p?.Trim();
                if (!string.IsNullOrEmpty(id))
                    yield return id;
            }
        }

        /// <summary>
        /// Возвращает узел из словаря по id или создаёт новый заглушечный NewDataItem и добавляет его в словарь.
        /// Новый узел имеет заполненное ID и пустой/инициализированный Children.
        /// </summary>
        private static NewDataItem GetOrCreateNode(Dictionary<string, NewDataItem> dict, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            if (dict.TryGetValue(id, out var node))
                return node;

            // Создаём "заглушку"
            node = new NewDataItem
            {
                ID = id,
                LONG_PATH = id,
                DESCRIPTION = string.Empty,
                // убедимся, что Children и остальные поля не null
                Children = new List<NewDataItem>(),
                Parent = null,
                Root = false
            };

            dict[id] = node;
            return node;
        }

        public List<Element> ConvertTreeToElements(IEnumerable<NewDataItem> roots)
        {
            if (roots == null) throw new ArgumentNullException(nameof(roots));

            // Словарь по ссылкам: каждый NewDataItem -> соответствующий Element
            var map = new Dictionary<NewDataItem, Element>(new ReferenceEqualityComparer<NewDataItem>());

            var resultRoots = new List<Element>();

            int rootIndex = 1;
            foreach (var r in roots)
            {
                if (r == null) continue;
                // позиция корня: "001", "002", ...
                var rootPos = rootIndex.ToString("000");
                var elRoot = ConvertNodeRecursive(r, null, map, rootPos);
                if (elRoot != null)
                    resultRoots.Add(elRoot);
                rootIndex++;
            }

            return resultRoots;
        }

        /// <summary>
        /// Преобразует дерево Element (список корней) в плоский список элементов.
        /// По умолчанию возвращает каждый Element только один раз (unique = true).
        /// Если unique = false, возвращаются все вхождения в порядке preorder обхода.
        /// </summary>
        public List<Element> FlattenElements(IEnumerable<Element> roots, bool unique = false)
        {
            if (roots == null) throw new ArgumentNullException(nameof(roots));

            var result = new List<Element>();

            // HashSet для уникализации по ссылке (если нужно)
            HashSet<Element> visited = unique ? new HashSet<Element>(new ReferenceEqualityComparer<Element>()) : null;

            // Используем стек для итеративного DFS (pre-order)
            var stack = new Stack<Element>(roots.Reverse()); // Reverse чтобы первый root обрабатывался первым

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (cur == null) continue;

                if (unique)
                {
                    if (!visited.Add(cur))
                        continue; // уже встречали — пропускаем
                }

                result.Add(cur);

                if (cur.Children == null || cur.Children.Count == 0) continue;

                // Добавляем детей в стек в обратном порядке, чтобы сохранить порядок обхода слева направо
                for (int i = cur.Children.Count - 1; i >= 0; i--)
                {
                    var child = cur.Children[i];
                    if (child != null)
                        stack.Push(child);
                }
            }

            return result;
        }

        /// <summary>
        /// Рекурсивная конвертация узла; pos — уже вычисленная позиция для текущего узла.
        /// Не перезаписывает Pos у уже созданного Element (если узел встречается повторно, первая позиция сохраняется).
        /// </summary>
        private Element ConvertNodeRecursive(NewDataItem node, Element parent, Dictionary<NewDataItem, Element> map, string pos)
        {
            if (node == null) return null;

            // Если уже конвертировали эту ссылку — вернём существующий элемент.
            // Не перезаписываем Pos у уже созданного элемента.
            if (map.TryGetValue(node, out var existing))
            {
                // Установим parent, если ещё не было (на случай множественных вхождений)
                if (existing.Parent == null && parent != null)
                {
                    existing.Parent = parent;
                    if (parent.Children == null) parent.Children = new List<Element>();
                    if (!parent.Children.Contains(existing)) parent.Children.Add(existing);

                    existing.Parents = new List<Element>(parent.Parents ?? new List<Element>());
                    existing.Root = existing.Parent == null;
                }
                return existing;
            }

            // Создаём новый Element и маппим поля (поля можете расширить/переопределить)
            var el = new Element();

            // === Присвоение полей (вы можете изменить/дополнить)
            el.Pos = pos; // установка позиции
            el.Designation = node.ID;
            el.Name = node.DESCRIPTION;
            el.FileName = node.FILE_NAME;
            el.MakeOrBuy = node.MAKE_BUY;
            el.Revision = node.REVISION;
            if (el.Revision == " ")
                el.Revision = "00";
            el.ItemCodeSupplier = node.ITEM_CODE_SUPPLIER;
            el.Costtype = node.COSTTYPE;
            el.Spare = node.SPARES;
            el.AddInfo = node.ADD_INFO;
            el.HenconStatus = node.HENCON_STD;
            el.Quantity = node.Quantity;

            if (el.Designation == "440023755")
            {

            }

            if (node.Type == "A" || node.Type == "Assy")
            {
                el.DrawingIcon = "Assy";
                el.TreeType = ElementConstants.ASSEMBLY;
            }
            else if (node.Type == "P" || node.Type == "Part")
            {
                el.TreeType = "Part";
                el.DrawingIcon = ElementConstants.DETAIL;
            }
            else if (node.Type == "G" || node.Type == "Generic")
            {
                el.TreeType = "BOM Item";
                el.DrawingIcon = ElementConstants.GENERIC;
            }

            if (node.BOM != " " && !string.IsNullOrEmpty(node.BOM))
            {
                el.DrawingIcon = ElementConstants.BOM;
                el.TreeType = "BOM Item";
            }

            // Связи
            el.Parent = parent;
            el.Root = parent == null;

            // Parents (от корня к непосредственному родителю)
            if (parent == null)
                el.Parents = new List<Element>();
            else
            {
                el.Parents = new List<Element>(parent.Parents ?? new List<Element>());
                el.Parents.Add(parent);
            }

            el.Children = new List<Element>();

            // Помещаем в словарь по ссылке (чтобы повторные вхождения реиспользовали этот Element)
            map[node] = el;

            // Обрабатываем детей и строим их Pos: childPos = "{pos}.{000}"
            if (node.Children != null && node.Children.Count > 0)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    var childNode = node.Children[i];
                    if (childNode == null) continue;

                    // позиция ребенка: например, для root "001" -> дети "001.001", "001.002"
                    var childPos = $"{pos}.{(i + 1).ToString("000")}";

                    var childEl = ConvertNodeRecursive(childNode, el, map, childPos);

                    if (childEl != null && !el.Children.Contains(childEl))
                        el.Children.Add(childEl);
                }
            }

            return el;
        }

        // Простой comparer, сравнивающий ссылки (надёжно для словаря по объектам)
        private class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}