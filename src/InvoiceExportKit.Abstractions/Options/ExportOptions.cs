namespace InvoiceExportKit.Abstractions.Options;

/// <summary>Controls behaviour of the export pipeline.</summary>
public sealed class ExportOptions
{
    /// <summary>
    /// Name of the template to apply (e.g. "Simple", "Corporate").
    /// Defaults to <c>"Simple"</c>.
    /// </summary>
    public string TemplateName { get; set; } = "Simple";

    /// <summary>
    /// When <c>true</c> the exporter recalculates line totals and invoice totals
    /// before rendering, overwriting any pre-existing values.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool RecalculateTotals { get; set; } = true;

    /// <summary>
    /// Name of the worksheet tab shown in Excel.
    /// Defaults to <c>"Invoice"</c>.
    /// </summary>
    public string SheetName { get; set; } = "Invoice";

    /// <summary>
    /// When <c>true</c>, a notes row is rendered at the bottom of the sheet
    /// even when <see cref="Models.InvoiceModel.Notes"/> is empty.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool AlwaysShowNotesRow { get; set; } = false;
}
