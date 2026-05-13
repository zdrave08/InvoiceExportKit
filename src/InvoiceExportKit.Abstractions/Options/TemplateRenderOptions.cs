namespace InvoiceExportKit.Abstractions.Options;

/// <summary>Visual parameters consumed by the Open XML renderer.</summary>
public sealed class TemplateRenderOptions
{
    /// <summary>ARGB hex colour for the main header band (invoice title row).</summary>
    public string HeaderBackgroundColor { get; set; } = "366092";

    /// <summary>ARGB hex font colour used inside the header band.</summary>
    public string HeaderFontColor { get; set; } = "FFFFFF";

    /// <summary>ARGB hex colour for section-label rows (SELLER / BUYER / column headers).</summary>
    public string SubheaderBackgroundColor { get; set; } = "D9E2F3";

    /// <summary>ARGB hex font colour used for section labels.</summary>
    public string SubheaderFontColor { get; set; } = "17375E";

    /// <summary>ARGB hex colour for alternating item rows (even rows). Empty string disables banding.</summary>
    public string AlternateRowColor { get; set; } = string.Empty;

    /// <summary>Font size (pt) for the "INVOICE" title cell.</summary>
    public double TitleFontSize { get; set; } = 20;

    /// <summary>Default body font size (pt).</summary>
    public double BodyFontSize { get; set; } = 10;

    /// <summary>Font family name used throughout the worksheet.</summary>
    public string FontName { get; set; } = "Calibri";

    /// <summary>
    /// Column widths keyed by 1-based column index.
    /// Any column not present falls back to the Excel default.
    /// </summary>
    public Dictionary<int, double> ColumnWidths { get; set; } = new();

    /// <summary>When <c>true</c> the totals row is rendered with a heavier top border.</summary>
    public bool EmphasizeTotalRow { get; set; } = true;
}
