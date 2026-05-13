using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace InvoiceExportKit.Excel.OpenXml.Helpers;

/// <summary>Factory helpers for creating Open XML <see cref="Cell"/> instances.</summary>
internal static class CellHelper
{
    // Excel's date epoch (with the intentional 1900 leap-year bug accounted for).
    private static readonly DateTime ExcelEpoch = new(1899, 12, 30);

    /// <summary>Creates a string cell using an inline string value.</summary>
    public static Cell CreateTextCell(string cellReference, string? value, uint styleIndex = 0)
    {
        return new Cell
        {
            CellReference = cellReference,
            DataType = CellValues.InlineString,
            StyleIndex = styleIndex,
            InlineString = new InlineString { Text = new Text(value ?? string.Empty) }
        };
    }

    /// <summary>Creates a numeric cell.</summary>
    public static Cell CreateNumberCell(string cellReference, decimal value, uint styleIndex = 0)
    {
        return new Cell
        {
            CellReference = cellReference,
            DataType = CellValues.Number,
            StyleIndex = styleIndex,
            CellValue = new CellValue(value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture))
        };
    }

    /// <summary>Creates a date cell encoded as an OLE Automation number.</summary>
    public static Cell CreateDateCell(string cellReference, DateTime value, uint styleIndex = 0)
    {
        double oaDate = (value.Date - ExcelEpoch).TotalDays;
        return new Cell
        {
            CellReference = cellReference,
            DataType = CellValues.Number,
            StyleIndex = styleIndex,
            CellValue = new CellValue(oaDate.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
    }

    /// <summary>Creates an empty placeholder cell (preserves column position in the row).</summary>
    public static Cell CreateEmptyCell(string cellReference, uint styleIndex = 0)
    {
        return new Cell
        {
            CellReference = cellReference,
            StyleIndex = styleIndex
        };
    }

    /// <summary>Returns the Excel column letter(s) for a 1-based column index.</summary>
    public static string ColumnName(int columnIndex)
    {
        string name = string.Empty;
        while (columnIndex > 0)
        {
            int rem = (columnIndex - 1) % 26;
            name = (char)('A' + rem) + name;
            columnIndex = (columnIndex - 1) / 26;
        }
        return name;
    }

    /// <summary>Returns the cell reference string (e.g. "C5") for 1-based column and row indices.</summary>
    public static string Ref(int col, int row) => $"{ColumnName(col)}{row}";
}
