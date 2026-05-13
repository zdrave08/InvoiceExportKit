using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;
using InvoiceExportKit.Excel.OpenXml.Helpers;
using InvoiceExportKit.Templates;

namespace InvoiceExportKit.Excel.OpenXml.Rendering;

/// <summary>
/// Exports an <see cref="InvoiceModel"/> to an <c>.xlsx</c> byte array
/// using the Open XML SDK. This class is format-specific and has no dependency
/// on business logic or validation — use <c>InvoiceExportService</c> for the full pipeline.
/// </summary>
public sealed class ExcelInvoiceExporter : IInvoiceExporter
{
    private readonly IEnumerable<IInvoiceTemplate>? _customTemplates;

    /// <param name="customTemplates">
    /// Optional additional templates to register alongside the built-in ones.
    /// </param>
    public ExcelInvoiceExporter(IEnumerable<IInvoiceTemplate>? customTemplates = null)
    {
        _customTemplates = customTemplates;
    }

    /// <inheritdoc/>
    public byte[] Export(InvoiceModel invoice, ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        options ??= new ExportOptions();

        IInvoiceTemplate template = ResolveTemplate(options.TemplateName);
        TemplateRenderOptions renderOpts = template.RenderOptions;

        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new StylesheetBuilder(renderOpts).Build();
            stylesPart.Stylesheet.Save();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            var composer = new WorksheetComposer(invoice, renderOpts);
            var (sheetData, mergeCells, columns) = composer.Compose();

            var worksheet = new Worksheet();
            worksheet.Append(columns);
            worksheet.Append(sheetData);
            worksheet.Append(mergeCells);
            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Save();

            string sheetName = string.IsNullOrWhiteSpace(options.SheetName) ? "Invoice" : options.SheetName;
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = sheetName
            });

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private IInvoiceTemplate ResolveTemplate(string? name)
    {
        if (_customTemplates is not null)
        {
            var custom = _customTemplates.FirstOrDefault(
                t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
            if (custom is not null) return custom;
        }

        return TemplateRegistry.Resolve(name);
    }
}
