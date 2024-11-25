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
                _excelReader = _factory.Create(initialData.ExcelFile);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private void EnsureReaderInitialized()
        {
            if (_excelReader != null || string.IsNullOrEmpty(_initialData.ExcelFile)) return;

            try
            {
                _excelReader = _factory.Create(_initialData.ExcelFile);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private void ProcessColumns(int sheetNumber, int headerRowNumber)
        {
            try
            {
                for (int col = 1; col <= 40; col++)
                {
                    string columnName = ExcelColumnFromNumber(col);
                    string cellValue = _excelReader.ReadCell(sheetNumber, columnName, headerRowNumber);
                    AssignColumnNames(cellValue, columnName);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        public void PrepareData(string filePath)
        {
            try
            {
                EnsureReaderInitialized();

                var headerRowNumber = GetHeaderRowNumber();
                var sheetNumber = DefaultReadSettings.LIST_NUMBER_DEFAULT;

                ProcessColumns(sheetNumber, headerRowNumber);

                var lastRow = GetLastUsedRow();
                _initialData.EndRow = lastRow;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private int GetLastUsedRow()
        {
            try
            {
                var lastRow = _excelReader.GetLastUsedRow(_initialData.SheetNumber,
                                                          _initialData.DesignationColumn,
                                                          _initialData.StartRow);
                return lastRow;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private int GetHeaderRowNumber()
        {
            return _initialData.HeaderRow == 0 ? DefaultReadSettings.HEADER_ROW_NUMBER : _initialData.HeaderRow;
        }

        private void AssignColumnNames(string cellValue, string columnName)
        {
            try
            {
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
                }
                else
                {
                    AppLogger.LogError(new Exception($"Неизвестный столбец: {cellValue}"), ErrorMessages.InvalidExcelFilePath);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }

        private string ExcelColumnFromNumber(int column)
        {
            try
            {
                string columnString = string.Empty;
                while (column > 0)
                {
                    int currentLetterNumber = (column - 1) % 26;
                    char currentLetter = (char)(currentLetterNumber + 65);
                    columnString = currentLetter + columnString;
                    column = (column - (currentLetterNumber + 1)) / 26;
                }
                return columnString;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, ErrorMessages.InitialDataSetterServiceError);
                throw new InvalidOperationException(ErrorMessages.InitialDataSetterServiceError, ex);
            }
        }
    }
}
