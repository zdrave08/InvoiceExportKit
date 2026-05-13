using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Abstractions.Options;

namespace InvoiceExportKit.Templates;

/// <summary>
/// A clean, minimal invoice template with a light-blue header and no row banding.
/// Suitable for general-purpose use.
/// </summary>
public sealed class SimpleInvoiceTemplate : IInvoiceTemplate
{
    /// <inheritdoc/>
    public string Name => "Simple";

    /// <inheritdoc/>
    public TemplateRenderOptions RenderOptions { get; } = new()
    {
        HeaderBackgroundColor = "4472C4",
        HeaderFontColor = "FFFFFF",
        SubheaderBackgroundColor = "D9E2F3",
        SubheaderFontColor = "1F3864",
        AlternateRowColor = string.Empty,
        TitleFontSize = 18,
        BodyFontSize = 10,
        FontName = "Calibri",
        EmphasizeTotalRow = true,
        ColumnWidths = new Dictionary<int, double>
        {
            { 1, 5 },   // #
            { 2, 14 },  // Code
            { 3, 36 },  // Description
            { 4, 9 },   // Qty
            { 5, 9 },   // Unit
            { 6, 14 },  // Unit Price
            { 7, 11 },  // Disc %
            { 8, 16 }   // Line Total
        }
    };
}
