using DocumentFormat.OpenXml.Spreadsheet;
using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;
using InvoiceExportKit.Excel.OpenXml.Helpers;
using static InvoiceExportKit.Excel.OpenXml.Helpers.StylesheetBuilder;
using static InvoiceExportKit.Excel.OpenXml.Helpers.CellHelper;

namespace InvoiceExportKit.Excel.OpenXml.Rendering;

/// <summary>
/// Composes a fully populated <see cref="SheetData"/> and associated <see cref="MergeCells"/>
/// and <see cref="Columns"/> for an invoice, given a set of <see cref="TemplateRenderOptions"/>.
/// </summary>
internal sealed class WorksheetComposer
{
    private const int TotalColumns = 8;

    private readonly InvoiceModel _invoice;
    private readonly TemplateRenderOptions _opts;
    private readonly List<MergeCell> _merges = new();
    private int _row = 1;

    public WorksheetComposer(InvoiceModel invoice, TemplateRenderOptions opts)
    {
        _invoice = invoice;
        _opts = opts;
    }

    public (SheetData data, MergeCells merges, Columns columns) Compose()
    {
        var data = new SheetData();

        AddTitleRow(data);
        AddBlankRow(data);
        AddInvoiceMetaRows(data);
        AddBlankRow(data);
        AddSellerBuyerSection(data);
        AddBlankRow(data);
        AddItemsTable(data);
        AddBlankRow(data);
        AddTotalsSection(data);

        if (!string.IsNullOrWhiteSpace(_invoice.Notes) || _opts.EmphasizeTotalRow)
        {
            AddBlankRow(data);
            AddNotesRow(data);
        }

        return (data, BuildMergeCells(), BuildColumns());
    }

    // ── Title ────────────────────────────────────────────────────────────────

    private void AddTitleRow(SheetData data)
    {
        var row = new Row { RowIndex = (uint)_row, Height = 36, CustomHeight = true };
        row.Append(CreateTextCell(Ref(1, _row), "INVOICE", StyleTitle));
        for (int c = 2; c <= TotalColumns; c++)
            row.Append(CreateEmptyCell(Ref(c, _row), StyleTitle));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"A{_row}:H{_row}" });
        _row++;
    }

    // ── Invoice metadata ─────────────────────────────────────────────────────

    private void AddInvoiceMetaRows(SheetData data)
    {
        AddLabelValueRow(data, "Invoice No:", _invoice.InvoiceNumber, StyleBold, StyleDefault);
        AddLabelDateRow(data, "Issue Date:", _invoice.IssueDate);
        AddLabelDateRow(data, "Due Date:", _invoice.DueDate);
        AddLabelValueRow(data, "Currency:", _invoice.Currency, StyleBold, StyleDefault);
    }

    private void AddLabelValueRow(SheetData data, string label, string value,
        uint labelStyle, uint valueStyle)
    {
        var row = MakeRow();
        row.Append(CreateTextCell(Ref(1, _row), label, labelStyle));
        row.Append(CreateTextCell(Ref(2, _row), value, valueStyle));
        for (int c = 3; c <= TotalColumns; c++)
            row.Append(CreateEmptyCell(Ref(c, _row)));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"B{_row}:D{_row}" });
        _row++;
    }

    private void AddLabelDateRow(SheetData data, string label, DateTime date)
    {
        var row = MakeRow();
        row.Append(CreateTextCell(Ref(1, _row), label, StyleBold));
        row.Append(CreateDateCell(Ref(2, _row), date, StyleDate));
        for (int c = 3; c <= TotalColumns; c++)
            row.Append(CreateEmptyCell(Ref(c, _row)));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"B{_row}:D{_row}" });
        _row++;
    }

    // ── Seller / Buyer ───────────────────────────────────────────────────────

    private void AddSellerBuyerSection(SheetData data)
    {
        AddSectionHeaderRow(data, "SELLER", "BUYER");

        var sellerLines = ContactLines(_invoice.Seller);
        var buyerLines  = ContactLines(_invoice.Buyer);
        int count = Math.Max(sellerLines.Count, buyerLines.Count);

        for (int i = 0; i < count; i++)
        {
            string? sl = i < sellerLines.Count ? sellerLines[i] : null;
            string? bl = i < buyerLines.Count  ? buyerLines[i]  : null;
            AddTwoColumnRow(data, sl, bl);
        }
    }

    private void AddSectionHeaderRow(SheetData data, string left, string right)
    {
        var row = MakeRow();
        row.Append(CreateTextCell(Ref(1, _row), left,  StyleSectionHeader));
        row.Append(CreateTextCell(Ref(2, _row), null,  StyleSectionHeader));
        row.Append(CreateTextCell(Ref(3, _row), null,  StyleSectionHeader));
        row.Append(CreateTextCell(Ref(4, _row), null,  StyleSectionHeader));
        row.Append(CreateTextCell(Ref(5, _row), right, StyleSectionHeader));
        for (int c = 6; c <= TotalColumns; c++)
            row.Append(CreateTextCell(Ref(c, _row), null, StyleSectionHeader));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"A{_row}:D{_row}" });
        _merges.Add(new MergeCell { Reference = $"E{_row}:H{_row}" });
        _row++;
    }

    private void AddTwoColumnRow(SheetData data, string? left, string? right)
    {
        var row = MakeRow();
        row.Append(CreateTextCell(Ref(1, _row), left,  StyleDefault));
        row.Append(CreateEmptyCell(Ref(2, _row)));
        row.Append(CreateEmptyCell(Ref(3, _row)));
        row.Append(CreateEmptyCell(Ref(4, _row)));
        row.Append(CreateTextCell(Ref(5, _row), right, StyleDefault));
        for (int c = 6; c <= TotalColumns; c++)
            row.Append(CreateEmptyCell(Ref(c, _row)));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"A{_row}:D{_row}" });
        _merges.Add(new MergeCell { Reference = $"E{_row}:H{_row}" });
        _row++;
    }

    private static List<string> ContactLines(ContactModel contact)
    {
        var lines = new List<string> { contact.Name };
        if (!string.IsNullOrWhiteSpace(contact.TaxId))
            lines.Add($"Tax ID: {contact.TaxId}");
        if (!string.IsNullOrWhiteSpace(contact.Address?.Street))
            lines.Add(contact.Address.Street);
        string cityLine = string.Join(", ", new[]
            {
                contact.Address?.PostalCode,
                contact.Address?.City,
                contact.Address?.Country
            }.Where(s => !string.IsNullOrWhiteSpace(s))!);
        if (!string.IsNullOrWhiteSpace(cityLine))
            lines.Add(cityLine);
        if (!string.IsNullOrWhiteSpace(contact.Email))
            lines.Add(contact.Email);
        if (!string.IsNullOrWhiteSpace(contact.Phone))
            lines.Add(contact.Phone);
        return lines;
    }

    // ── Items table ──────────────────────────────────────────────────────────

    private void AddItemsTable(SheetData data)
    {
        AddColumnHeaderRow(data);

        int itemIndex = 1;
        foreach (var item in _invoice.Items)
        {
            bool isAlt = !string.IsNullOrWhiteSpace(_opts.AlternateRowColor) && itemIndex % 2 == 0;
            uint textStyle    = isAlt ? StyleAltRow         : StyleTableText;
            uint numberStyle  = isAlt ? StyleAltRowNumber   : StyleTableNumber;
            uint currencyStyle = isAlt ? StyleAltRowCurrency : StyleTableCurrency;

            var row = MakeRow();
            row.Append(CreateNumberCell(Ref(1, _row), itemIndex,          textStyle));
            row.Append(CreateTextCell(  Ref(2, _row), item.Code ?? "",    textStyle));
            row.Append(CreateTextCell(  Ref(3, _row), item.Description,   textStyle));
            row.Append(CreateNumberCell(Ref(4, _row), item.Quantity,      numberStyle));
            row.Append(CreateTextCell(  Ref(5, _row), item.Unit,          textStyle));
            row.Append(CreateNumberCell(Ref(6, _row), item.UnitPrice,     currencyStyle));
            row.Append(CreateNumberCell(Ref(7, _row), item.DiscountPercent, numberStyle));
            row.Append(CreateNumberCell(Ref(8, _row), item.LineTotal,     currencyStyle));
            data.Append(row);
            _row++;
            itemIndex++;
        }
    }

    private void AddColumnHeaderRow(SheetData data)
    {
        var row = MakeRow();
        string[] headers = { "#", "Code", "Description", "Qty", "Unit", "Unit Price", "Disc %", "Total" };
        for (int c = 1; c <= TotalColumns; c++)
            row.Append(CreateTextCell(Ref(c, _row), headers[c - 1], StyleColumnHeader));
        data.Append(row);
        _row++;
    }

    // ── Totals ───────────────────────────────────────────────────────────────

    private void AddTotalsSection(SheetData data)
    {
        if (_invoice.Totals is null) return;

        var t = _invoice.Totals;
        string currency = _invoice.Currency;

        AddTotalsRow(data, "Subtotal",        t.Subtotal,        StyleBold, StyleBoldCurrency, false);
        if (t.DiscountAmount != 0)
            AddTotalsRow(data, "Discount",    t.DiscountAmount,  StyleBold, StyleBoldCurrency, false);
        if (t.TaxRate != 0)
            AddTotalsRow(data, $"Tax ({t.TaxRate:0.##}%)", t.TaxAmount, StyleBold, StyleBoldCurrency, false);
        AddTotalsRow(data, $"TOTAL ({currency})", t.Total, StyleBold, StyleBoldCurrency, _opts.EmphasizeTotalRow);
    }

    private void AddTotalsRow(SheetData data, string label, decimal amount,
        uint labelStyle, uint valueStyle, bool emphasize)
    {
        var row = MakeRow();
        for (int c = 1; c <= 6; c++)
            row.Append(CreateEmptyCell(Ref(c, _row)));
        row.Append(CreateTextCell(Ref(7, _row), label,  labelStyle));
        row.Append(CreateNumberCell(Ref(8, _row), amount, valueStyle));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"A{_row}:F{_row}" });
        _row++;
    }

    // ── Notes ────────────────────────────────────────────────────────────────

    private void AddNotesRow(SheetData data)
    {
        if (string.IsNullOrWhiteSpace(_invoice.Notes)) return;

        var row = MakeRow();
        row.Append(CreateTextCell(Ref(1, _row), "Notes:", StyleBold));
        row.Append(CreateTextCell(Ref(2, _row), _invoice.Notes, StyleDefault));
        for (int c = 3; c <= TotalColumns; c++)
            row.Append(CreateEmptyCell(Ref(c, _row)));
        data.Append(row);
        _merges.Add(new MergeCell { Reference = $"B{_row}:H{_row}" });
        _row++;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AddBlankRow(SheetData data)
    {
        data.Append(new Row { RowIndex = (uint)_row });
        _row++;
    }

    private Row MakeRow() => new() { RowIndex = (uint)_row };

    private MergeCells BuildMergeCells()
    {
        var mc = new MergeCells();
        foreach (var m in _merges) mc.Append(m);
        mc.Count = (uint)_merges.Count;
        return mc;
    }

    private Columns BuildColumns()
    {
        var columns = new Columns();
        for (int c = 1; c <= TotalColumns; c++)
        {
            if (_opts.ColumnWidths.TryGetValue(c, out double width))
            {
                columns.Append(new Column
                {
                    Min = (uint)c,
                    Max = (uint)c,
                    Width = width,
                    CustomWidth = true
                });
            }
        }
        return columns;
    }
}
