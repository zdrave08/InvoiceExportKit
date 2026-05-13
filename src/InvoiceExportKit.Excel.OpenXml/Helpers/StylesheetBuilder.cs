using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using InvoiceExportKit.Abstractions.Options;

namespace InvoiceExportKit.Excel.OpenXml.Helpers;

/// <summary>
/// Builds an Open XML <see cref="Stylesheet"/> from <see cref="TemplateRenderOptions"/>.
/// Style index constants are exposed as public fields so the worksheet composer can
/// reference them without magic numbers.
/// </summary>
internal sealed class StylesheetBuilder
{
    // ── Style indices ────────────────────────────────────────────────────────
    public const uint StyleDefault        = 0;
    public const uint StyleBold           = 1;
    public const uint StyleTitle          = 2;
    public const uint StyleColumnHeader   = 3;
    public const uint StyleSectionHeader  = 4;
    public const uint StyleDate           = 5;
    public const uint StyleNumber         = 6;
    public const uint StyleCurrency       = 7;
    public const uint StyleBoldCurrency   = 8;
    public const uint StyleTableText      = 9;
    public const uint StyleTableNumber    = 10;
    public const uint StyleTableCurrency  = 11;
    public const uint StyleAltRow         = 12;
    public const uint StyleAltRowNumber   = 13;
    public const uint StyleAltRowCurrency = 14;

    // ── Custom number-format IDs (must be >= 164) ────────────────────────────
    private const uint FmtDate     = 164;
    private const uint FmtNumber2  = 165;
    private const uint FmtCurrency = 166;

    private readonly TemplateRenderOptions _opts;

    public StylesheetBuilder(TemplateRenderOptions opts) => _opts = opts;

    public Stylesheet Build()
    {
        return new Stylesheet(
            BuildNumberFormats(),
            BuildFonts(),
            BuildFills(),
            BuildBorders(),
            BuildCellStyleFormats(),
            BuildCellFormats()
        );
    }

    // ── Number formats ───────────────────────────────────────────────────────

    private static NumberingFormats BuildNumberFormats() => new(
        new NumberingFormat { NumberFormatId = FmtDate,     FormatCode = "dd/mm/yyyy" },
        new NumberingFormat { NumberFormatId = FmtNumber2,  FormatCode = "#,##0.00" },
        new NumberingFormat { NumberFormatId = FmtCurrency, FormatCode = "#,##0.00" }
    ) { Count = 3 };

    // ── Fonts ────────────────────────────────────────────────────────────────
    // Index 0 – default body
    // Index 1 – bold body
    // Index 2 – title (large bold)
    // Index 3 – column-header (bold, white)
    // Index 4 – section-header (bold, dark)

    private Fonts BuildFonts()
    {
        string font = _opts.FontName;
        double body = _opts.BodyFontSize;
        double title = _opts.TitleFontSize;

        return new Fonts(
            Font(font, body, null,                false),
            Font(font, body, null,                true),
            Font(font, title, null,               true),
            Font(font, body, _opts.HeaderFontColor,    true),
            Font(font, body, _opts.SubheaderFontColor, true)
        ) { Count = 5 };
    }

    private static DocumentFormat.OpenXml.Spreadsheet.Font Font(
        string name, double size, string? colorHex, bool bold)
    {
        var f = new DocumentFormat.OpenXml.Spreadsheet.Font(
            new FontSize { Val = size },
            new FontName { Val = name }
        );
        if (bold) f.Append(new Bold());
        if (colorHex is not null)
            f.Append(new Color { Rgb = new HexBinaryValue(colorHex) });
        return f;
    }

    // ── Fills ────────────────────────────────────────────────────────────────
    // OpenXML requires index 0 = none, index 1 = gray (reserved).
    // Index 2 – header background
    // Index 3 – sub-header background
    // Index 4 – alternate row (empty string = same as none)

    private Fills BuildFills()
    {
        var fills = new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
            SolidFill(_opts.HeaderBackgroundColor),
            SolidFill(_opts.SubheaderBackgroundColor)
        );

        string alt = _opts.AlternateRowColor;
        fills.Append(string.IsNullOrWhiteSpace(alt)
            ? new Fill(new PatternFill { PatternType = PatternValues.None })
            : SolidFill(alt));

        fills.Count = 5;
        return fills;
    }

    private static Fill SolidFill(string hex) =>
        new(new PatternFill(
            new ForegroundColor { Rgb = new HexBinaryValue(hex) },
            new BackgroundColor { Indexed = 64 }
        ) { PatternType = PatternValues.Solid });

    // ── Borders ──────────────────────────────────────────────────────────────
    // Index 0 – none
    // Index 1 – all-sides thin (table cells)
    // Index 2 – top-medium + bottom-thin (total row)

    private static Borders BuildBorders() => new(
        new Border(
            new LeftBorder(), new RightBorder(), new TopBorder(),
            new BottomBorder(), new DiagonalBorder()),
        new Border(
            new LeftBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new RightBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new TopBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new BottomBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new DiagonalBorder()),
        new Border(
            new LeftBorder(),
            new RightBorder(),
            new TopBorder(new Color { Auto = true }) { Style = BorderStyleValues.Medium },
            new BottomBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new DiagonalBorder())
    ) { Count = 3 };

    // ── Cell style formats (xf in cellStyleXfs — base formats) ──────────────

    private static CellStyleFormats BuildCellStyleFormats() =>
        new(new CellFormat()) { Count = 1 };

    // ── Cell formats (xf in cellXfs) ─────────────────────────────────────────
    // Must match the StyleXxx constants declared at the top.

    private Formats BuildCellFormats()
    {
        // xfId=0 references the single base format.
        CellFormat Fmt(uint fontId, uint fillId, uint borderId,
                       uint? numFmtId = null, bool wrapText = false,
                       HorizontalAlignmentValues? halign = null)
        {
            var cf = new CellFormat
            {
                FontId = fontId,
                FillId = fillId,
                BorderId = borderId,
                FormatId = 0,
                ApplyFont = fontId != 0,
                ApplyFill = fillId != 0,
                ApplyBorder = borderId != 0
            };

            if (numFmtId.HasValue)
            {
                cf.NumberFormatId = numFmtId.Value;
                cf.ApplyNumberFormat = true;
            }

            if (halign.HasValue || wrapText)
            {
                cf.Alignment = new Alignment
                {
                    Horizontal = halign ?? HorizontalAlignmentValues.General,
                    WrapText = wrapText,
                    Vertical = VerticalAlignmentValues.Center
                };
                cf.ApplyAlignment = true;
            }

            return cf;
        }

        // Whether alternate row fill differs from default.
        bool hasAlt = !string.IsNullOrWhiteSpace(_opts.AlternateRowColor);

        return new Formats(
            /* 0  Default         */ Fmt(0, 0, 0),
            /* 1  Bold            */ Fmt(1, 0, 0),
            /* 2  Title           */ Fmt(2, 2, 0, halign: HorizontalAlignmentValues.Center),
            /* 3  ColumnHeader    */ Fmt(3, 2, 1, halign: HorizontalAlignmentValues.Center),
            /* 4  SectionHeader   */ Fmt(4, 3, 0),
            /* 5  Date            */ Fmt(0, 0, 0, FmtDate),
            /* 6  Number          */ Fmt(0, 0, 0, FmtNumber2, halign: HorizontalAlignmentValues.Right),
            /* 7  Currency        */ Fmt(0, 0, 0, FmtCurrency, halign: HorizontalAlignmentValues.Right),
            /* 8  BoldCurrency    */ Fmt(1, 0, 0, FmtCurrency, halign: HorizontalAlignmentValues.Right),
            /* 9  TableText       */ Fmt(0, 0, 1, wrapText: true),
            /* 10 TableNumber     */ Fmt(0, 0, 1, FmtNumber2, halign: HorizontalAlignmentValues.Right),
            /* 11 TableCurrency   */ Fmt(0, 0, 1, FmtCurrency, halign: HorizontalAlignmentValues.Right),
            /* 12 AltRow          */ Fmt(0, hasAlt ? 4u : 0u, 1, wrapText: true),
            /* 13 AltRowNumber    */ Fmt(0, hasAlt ? 4u : 0u, 1, FmtNumber2, halign: HorizontalAlignmentValues.Right),
            /* 14 AltRowCurrency  */ Fmt(0, hasAlt ? 4u : 0u, 1, FmtCurrency, halign: HorizontalAlignmentValues.Right)
        ) { Count = 15 };
    }
}
