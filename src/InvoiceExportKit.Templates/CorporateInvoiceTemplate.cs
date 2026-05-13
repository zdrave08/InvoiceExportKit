using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Abstractions.Options;

namespace InvoiceExportKit.Templates;

/// <summary>
/// A professional corporate template with a dark header, alternating row banding,
/// and a wider description column suitable for detailed line items.
/// </summary>
public sealed class CorporateInvoiceTemplate : IInvoiceTemplate
{
    /// <inheritdoc/>
    public string Name => "Corporate";

    /// <inheritdoc/>
    public TemplateRenderOptions RenderOptions { get; } = new()
    {
        HeaderBackgroundColor = "1F3864",
        HeaderFontColor = "FFFFFF",
        SubheaderBackgroundColor = "BDD7EE",
        SubheaderFontColor = "1F3864",
        AlternateRowColor = "EEF3FB",
        TitleFontSize = 20,
        BodyFontSize = 10,
        FontName = "Calibri",
        EmphasizeTotalRow = true,
        ColumnWidths = new Dictionary<int, double>
        {
            { 1, 5 },   // #
            { 2, 15 },  // Code
            { 3, 40 },  // Description
            { 4, 9 },   // Qty
            { 5, 9 },   // Unit
            { 6, 14 },  // Unit Price
            { 7, 11 },  // Disc %
            { 8, 17 }   // Line Total
        }
    };
}
