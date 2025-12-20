using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public class XlsxFileConverter : IFileConverter
{
    public bool CanConvert(string fileName)
    {
        return fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    public string? ConvertToMarkdown(string fileName)
    {
        try
        {
            return TryConvertToMarkdown(fileName);
        }
        catch (Exception ex)
        {
            var couldBeEncrypted = ex.Message.Contains("corrupted");
            if (couldBeEncrypted) return $"File encrypted or corrupted: {fileName}\n\nPlease remove encryption or fix the file and try again.";
            throw;
        }
    }

    private string TryConvertToMarkdown(string fileName)
    {
        using var spreadsheetDoc = SpreadsheetDocument.Open(fileName, false);
        var workbookPart = spreadsheetDoc.WorkbookPart;
        if (workbookPart == null) return string.Empty;

        var sb = new StringBuilder();
        var sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>();
        if (sheets == null || !sheets.Any()) return string.Empty;

        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

        foreach (var sheet in sheets)
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetName = sheet.Name?.Value ?? "Unnamed Sheet";
            
            sb.AppendLine(ConvertSheetToMarkdown(worksheetPart, sheetName, sharedStringTable));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ConvertSheetToMarkdown(WorksheetPart worksheetPart, string sheetName, SharedStringTable? sharedStringTable)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Sheet: {sheetName}");
        sb.AppendLine();

        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData == null) return sb.ToString();

        var rows = sheetData.Elements<Row>().ToList();
        if (!rows.Any()) return sb.ToString();

        // Extract cell data
        var cellData = ExtractCellData(rows, sharedStringTable);
        if (!cellData.Any()) return sb.ToString();

        // Find data bounds
        var (minRow, maxRow, minCol, maxCol) = FindDataBounds(cellData);

        // Build table view
        sb.AppendLine("## Table View");
        sb.AppendLine(BuildTableView(cellData, minRow, maxRow, minCol, maxCol));
        sb.AppendLine();

        // Build structured view
        sb.AppendLine("## Structured Data");
        sb.AppendLine("```yaml");
        sb.AppendLine($"sheet: {sheetName}");
        sb.AppendLine("cells:");
        sb.Append(BuildStructuredView(cellData));
        sb.AppendLine("```");

        return sb.ToString();
    }

    private Dictionary<string, CellInfo> ExtractCellData(List<Row> rows, SharedStringTable? sharedStringTable)
    {
        var cellData = new Dictionary<string, CellInfo>();

        foreach (var row in rows)
        {
            foreach (var cell in row.Elements<Cell>())
            {
                var cellReference = cell.CellReference?.Value;
                if (cellReference == null) continue;

                var cellValue = GetCellDisplayValue(cell, sharedStringTable);
                var cellInfo = new CellInfo
                {
                    Reference = cellReference,
                    Value = cellValue,
                    Formula = cell.CellFormula?.Text,
                    Type = DetermineCellType(cell)
                };

                // Only include cells that have actual content
                if (!string.IsNullOrWhiteSpace(cellValue) || !string.IsNullOrWhiteSpace(cellInfo.Formula))
                {
                    cellData[cellReference] = cellInfo;
                }
            }
        }

        return cellData;
    }

    private string GetCellDisplayValue(Cell cell, SharedStringTable? sharedStringTable)
    {
        var dataType = cell.DataType?.Value;

        // Handle inline strings (text stored directly in the cell)
        if (dataType == CellValues.InlineString)
        {
            // InlineString contains the text
            var inlineString = cell.InlineString;
            if (inlineString != null)
            {
                // Get all text elements and concatenate them
                var textElements = inlineString.Descendants<DocumentFormat.OpenXml.Spreadsheet.Text>();
                return string.Concat(textElements.Select(t => t.Text));
            }
        }

        // Handle shared strings (most common for text in Excel)
        if (dataType == CellValues.SharedString && sharedStringTable != null)
        {
            var value = cell.CellValue?.Text;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int index) && index >= 0)
            {
                var sharedStringItems = sharedStringTable.Elements<SharedStringItem>().ToList();
                if (index < sharedStringItems.Count)
                {
                    return sharedStringItems[index].InnerText;
                }
            }
        }

        // Handle regular values (numbers, dates, booleans)
        var cellValue = cell.CellValue?.Text;
        return cellValue ?? string.Empty;
    }

    private string DetermineCellType(Cell cell)
    {
        if (cell.CellFormula != null) return "formula";

        var dataType = cell.DataType?.Value;
        if (dataType == CellValues.SharedString || dataType == CellValues.String) return "string";
        if (dataType == CellValues.Boolean) return "boolean";
        if (dataType == CellValues.Date) return "date";
        if (dataType == CellValues.Number || cell.CellValue?.Text != null) return "number";

        return "string";
    }

    private (int minRow, int maxRow, int minCol, int maxCol) FindDataBounds(Dictionary<string, CellInfo> cellData)
    {
        var minRow = int.MaxValue;
        var maxRow = int.MinValue;
        var minCol = int.MaxValue;
        var maxCol = int.MinValue;

        foreach (var cellRef in cellData.Keys)
        {
            var (row, col) = ParseCellReference(cellRef);
            minRow = Math.Min(minRow, row);
            maxRow = Math.Max(maxRow, row);
            minCol = Math.Min(minCol, col);
            maxCol = Math.Max(maxCol, col);
        }

        return (minRow, maxRow, minCol, maxCol);
    }

    private (int row, int col) ParseCellReference(string cellRef)
    {
        // Parse "A1", "B2", "AA10", etc.
        var colStr = new string(cellRef.TakeWhile(char.IsLetter).ToArray());
        var rowStr = new string(cellRef.SkipWhile(char.IsLetter).ToArray());

        var row = int.Parse(rowStr);
        var col = ColumnNameToNumber(colStr);

        return (row, col);
    }

    private int ColumnNameToNumber(string columnName)
    {
        var col = 0;
        for (int i = 0; i < columnName.Length; i++)
        {
            col = col * 26 + (columnName[i] - 'A' + 1);
        }
        return col;
    }

    private string ColumnNumberToName(int col)
    {
        var columnName = new StringBuilder();
        while (col > 0)
        {
            var modulo = (col - 1) % 26;
            columnName.Insert(0, (char)('A' + modulo));
            col = (col - modulo) / 26;
        }
        return columnName.ToString();
    }

    private string BuildTableView(Dictionary<string, CellInfo> cellData, int minRow, int maxRow, int minCol, int maxCol)
    {
        var sb = new StringBuilder();

        // Build table rows
        for (int row = minRow; row <= maxRow; row++)
        {
            var rowValues = new List<string>();
            
            for (int col = minCol; col <= maxCol; col++)
            {
                var cellRef = $"{ColumnNumberToName(col)}{row}";
                var value = cellData.TryGetValue(cellRef, out var cellInfo) 
                    ? cellInfo.Value 
                    : string.Empty;
                rowValues.Add(value);
            }

            sb.AppendLine("| " + string.Join(" | ", rowValues) + " |");

            // Add separator after first row (header)
            if (row == minRow)
            {
                sb.AppendLine("|" + string.Join("|", rowValues.Select(_ => "---")) + "|");
            }
        }

        return sb.ToString();
    }

    private string BuildStructuredView(Dictionary<string, CellInfo> cellData)
    {
        var sb = new StringBuilder();

        foreach (var kvp in cellData.OrderBy(x => x.Key))
        {
            var cellRef = kvp.Key;
            var cellInfo = kvp.Value;

            // Format the cell entry
            sb.Append($"  {cellRef}: {{");

            var parts = new List<string>();

            if (!string.IsNullOrEmpty(cellInfo.Formula))
            {
                parts.Add($"formula: \"{cellInfo.Formula}\"");
                // If there's a value, include it; otherwise indicate it needs calculation
                if (!string.IsNullOrEmpty(cellInfo.Value))
                {
                    parts.Add($"value: {FormatValueForYaml(cellInfo.Value, cellInfo.Type)}");
                }
                else
                {
                    parts.Add("value: \"<needs calculation>\"");
                }
            }
            else
            {
                parts.Add($"value: {FormatValueForYaml(cellInfo.Value, cellInfo.Type)}");
            }

            parts.Add($"type: {cellInfo.Type}");

            sb.Append(string.Join(", ", parts));
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private string FormatValueForYaml(string value, string type)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        
        if (type == "string" || type == "date")
        {
            return $"\"{value}\"";
        }

        return value;
    }

    private class CellInfo
    {
        public string Reference { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Formula { get; set; }
        public string Type { get; set; } = "string";
    }
}
