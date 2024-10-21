using ExportTC.Constants;
using ExportTC.Model;
using ExportTC.Model.Factories;
using HenconExport;

public class ExcelDataImporter : IInitialDataSetter
{
    private IExcelReader _excelReader;
    private InitialData _initialData;
    private Lazy<IExcelReader>? _lazyExcelReader;
    private readonly IExcelReaderFactory _excelReaderFactory;

    public ExcelDataImporter(IExcelReaderFactory factory, InitialData initialData)
    {
        _excelReader = factory.Create(initialData.ExcelFile);
        _initialData = initialData;
    }


    public void PrepareData(string filePath)
    {
        var headerRowNumber = GetHeaderRowNumber();
        var sheetNumber = DefaultReadSettings.LIST_NUMBER_DEFAULT;

        ProcessColumns(sheetNumber, headerRowNumber);

        var lastRow = GetLastUsedRow();
        _initialData.EndRow = lastRow;
    }

    private int GetLastUsedRow()
    {
        var lastRow = _excelReader.GetLastUsedRow(_initialData.SheetNubmer,
                                                  _initialData.DesignationColumn,
                                                  _initialData.StartRow);
        return lastRow;
    }

    private int GetHeaderRowNumber()
    {
        return _initialData.HeaderRow == 0 ? DefaultReadSettings.HEADER_ROW_NUMBER : _initialData.HeaderRow;
    }

    private void ProcessColumns(int sheetNumber, int headerRowNumber)
    {
        for (int col = 1; col <= 30; col++)
        {
            string columnName = ExcelColumnFromNumber(col);
            string cellValue = _excelReader.ReadCell(sheetNumber, columnName, headerRowNumber);
            AssignColumnNames(cellValue, columnName);
        }
    }

    private void AssignColumnNames(string cellValue, string columnName)
    {
        if (cellValue == ColumnNameConstants.POS)
        {
            _initialData.PositionColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.DESIGNATION)
        {
            _initialData.DesignationColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.TQTY)
        {
            _initialData.QuantityColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.DESCRIPTION)
        {
            _initialData.DescriptionColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.MAKE_BUY)
        {
            _initialData.MakeBuyColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.MATERIAL)
        {
            _initialData.MaterialColumn = columnName;
        }
        else if (cellValue == ColumnNameConstants.REVISION)
        {
            _initialData.RevisionColumn = columnName;
        }
    }

    private string ExcelColumnFromNumber(int column)
    {
        string columnString = "";
        while (column > 0)
        {
            int currentLetterNumber = (column - 1) % 26;
            char currentLetter = (char)(currentLetterNumber + 65); 
            columnString = currentLetter + columnString;
            column = (column - (currentLetterNumber + 1)) / 26;
        }
        return columnString;
    }
}