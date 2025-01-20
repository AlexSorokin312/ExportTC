using ExportTC.Constants;
using ExportTC.Constants.ColumnNameConstants;
using ExportTC.Model.Factories;
using HenconExport;

namespace ExportTC.Model
{
    public class InitialDataSetter : IInitialDataSetter
    {
        private IExcelReader _excelReader;
        private readonly InitialData _initialData;
        private readonly IExcelReaderFactory _factory;

        public InitialDataSetter(IExcelReaderFactory factory, InitialData initialData)
        {
            _factory = factory;
            _initialData = initialData;
            try
            {
                LoggerDebug.LogInfo("Инициализация InitialDataSetter.");
                _excelReader = _factory.Create(initialData.ExcelFile);
                LoggerDebug.LogDebug("ExcelReader успешно создан.");
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при создании ExcelReader: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private void EnsureReaderInitialized()
        {
            if (_excelReader != null || string.IsNullOrEmpty(_initialData.ExcelFile)) return;

            try
            {
                LoggerDebug.LogInfo("Проверка инициализации ExcelReader.");
                _excelReader = _factory.Create(_initialData.ExcelFile);
                LoggerDebug.LogDebug("ExcelReader успешно инициализирован.");
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при повторной инициализации ExcelReader: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private void ProcessColumns(int sheetNumber, int headerRowNumber)
        {
            try
            {
                LoggerDebug.LogInfo($"Обработка столбцов: лист {sheetNumber}, строка заголовка {headerRowNumber}.");
                for (int col = 1; col <= 40; col++)
                {
                    string columnName = ExcelColumnFromNumber(col);
                    string cellValue = _excelReader.ReadCell(sheetNumber, columnName, headerRowNumber);
                    AssignColumnNames(cellValue, columnName);
                }
                LoggerDebug.LogDebug("Столбцы успешно обработаны.");
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при обработке столбцов: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        public void PrepareData(string filePath)
        {
            try
            {
                LoggerDebug.LogInfo("Подготовка данных начата.");
                EnsureReaderInitialized();

                var headerRowNumber = GetHeaderRowNumber();
                var sheetNumber = DefaultReadSettings.LIST_NUMBER_DEFAULT;

                ProcessColumns(sheetNumber, headerRowNumber);

                var lastRow = GetLastUsedRow();
                _initialData.EndRow = lastRow;
                LoggerDebug.LogInfo("Данные успешно подготовлены.");
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при подготовке данных: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private int GetLastUsedRow()
        {
            try
            {
                LoggerDebug.LogInfo("Определение последней использованной строки.");
                var lastRow = _excelReader.GetLastUsedRow(_initialData.SheetNumber,
                                                          _initialData.DesignationColumn,
                                                          _initialData.StartRow);
                LoggerDebug.LogDebug($"Последняя использованная строка: {lastRow}");
                return lastRow;
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при определении последней строки: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private int GetHeaderRowNumber()
        {
            var headerRow = _initialData.HeaderRow == 0 ? DefaultReadSettings.HEADER_ROW_NUMBER : _initialData.HeaderRow;
            LoggerDebug.LogDebug($"Номер строки заголовка: {headerRow}");
            return headerRow;
        }

        private void AssignColumnNames(string cellValue, string columnName)
        {
            try
            {
                LoggerDebug.LogInfo($"Назначение имени столбца: {cellValue}, колонка: {columnName}.");
                var columnMap = new Dictionary<string, Action<string>>
                {
                    { ColumnNameConstants.POS, value => _initialData.PositionColumn = columnName },
                    { ColumnNameConstants.DESIGNATION, value => _initialData.DesignationColumn = columnName },
                    { ColumnNameConstants.TQTY, value => _initialData.QuantityColumn = columnName },
                    { ColumnNameConstants.DESCRIPTION, value => _initialData.DescriptionColumn = columnName },
                    { ColumnNameConstants.MAKE_BUY, value => _initialData.MakeBuyColumn = columnName },
                    { ColumnNameConstants.MATERIAL, value => _initialData.MaterialColumn = columnName },
                    { ColumnNameConstants.REVISION, value => _initialData.RevisionColumn = columnName },
                    { ColumnNameConstants.ITEM_CODE_SUPPLIER, value => _initialData.ItemCodeSupplierColumn = columnName },
                    { ColumnNameConstants.COSTTYPE, value => _initialData.CosttypeColumn = columnName },
                    { ColumnNameConstants.SPARE, value => _initialData.SpareColumn = columnName },
                    { ColumnNameConstants.ADD_INFO, value => _initialData.AddInfoColumn = columnName }
                };

                if (columnMap.ContainsKey(cellValue))
                {
                    columnMap[cellValue](cellValue);
                    LoggerDebug.LogDebug($"Назначено значение для столбца: {cellValue} -> {columnName}");
                }
                else
                {
                    LoggerDebug.LogError($"Неизвестный столбец: {cellValue}");
                }
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при назначении имени столбца: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private string ExcelColumnFromNumber(int column)
        {
            try
            {
                LoggerDebug.LogInfo($"Преобразование номера столбца в имя: {column}");
                string columnString = string.Empty;
                while (column > 0)
                {
                    int currentLetterNumber = (column - 1) % 26;
                    char currentLetter = (char)(currentLetterNumber + 65);
                    columnString = currentLetter + columnString;
                    column = (column - (currentLetterNumber + 1)) / 26;
                }
                LoggerDebug.LogDebug($"Имя столбца: {columnString}");
                return columnString;
            }
            catch (Exception ex)
            {
                LoggerDebug.LogError($"Ошибка при преобразовании номера столбца: {ex.Message}");
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }
    }
}
