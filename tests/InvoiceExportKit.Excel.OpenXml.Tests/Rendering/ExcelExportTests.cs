using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;
using InvoiceExportKit.Core.Calculation;
using InvoiceExportKit.Excel.OpenXml.Rendering;
using Xunit;

namespace InvoiceExportKit.Excel.OpenXml.Tests.Rendering;

public sealed class ExcelExportTests
{
    private readonly ExcelInvoiceExporter _exporter = new();
    private readonly InvoiceTotalsCalculator _calculator = new();

    private static InvoiceModel BuildInvoice()
    {
        var inv = new InvoiceModel
        {
            InvoiceNumber = "TEST-2024-001",
            IssueDate     = new DateTime(2024, 6, 1),
            DueDate       = new DateTime(2024, 6, 30),
            Currency      = "EUR",
            TaxRate       = 20m,
            Notes         = "Test notes",
            Seller = new ContactModel
            {
                Name   = "Test Seller GmbH",
                TaxId  = "DE123456789",
                Email  = "info@seller.de",
                Address = new AddressModel
                {
                    Street = "Hauptstraße 1", City = "Berlin",
                    PostalCode = "10115", Country = "Germany"
                }
            },
            Buyer = new ContactModel
            {
                Name   = "Test Buyer Inc.",
                Email  = "info@buyer.com",
                Address = new AddressModel
                {
                    Street = "123 Main St", City = "London",
                    PostalCode = "SW1A 1AA", Country = "UK"
                }
            },
            Items =
            [
                new InvoiceItemModel
                {
                    Code        = "SKU-001",
                    Description = "Software License",
                    Quantity    = 1,
                    Unit        = "license",
                    UnitPrice   = 500m
                },
                new InvoiceItemModel
                {
                    Code            = "SKU-002",
                    Description     = "Support Hours",
                    Quantity        = 10,
                    Unit            = "hrs",
                    UnitPrice       = 80m,
                    DiscountPercent = 5m
                }
            ]
        };
        inv.Totals = new InvoiceTotalsCalculator().Calculate(inv);
        return inv;
    }

    [Fact]
    public void Export_ReturnsNonEmptyByteArray()
    {
        var invoice = BuildInvoice();
        byte[] result = _exporter.Export(invoice);

        Assert.NotNull(result);
        Assert.True(result.Length > 0, "Exported byte array should not be empty.");
    }

    [Fact]
    public void Export_ProducesValidZipArchive()
    {
        var invoice = BuildInvoice();
        byte[] result = _exporter.Export(invoice);

        // An .xlsx file is a ZIP archive. First 4 bytes are the ZIP magic number.
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public void Export_SpreadsheetDocumentCanBeOpened()
    {
        var invoice = BuildInvoice();
        byte[] result = _exporter.Export(invoice);

        using var stream = new MemoryStream(result);
        using var doc = SpreadsheetDocument.Open(stream, false);

        Assert.NotNull(doc.WorkbookPart);
        Assert.NotNull(doc.WorkbookPart.Workbook);
    }

    [Fact]
    public void Export_ContainsExpectedSheetName()
    {
        var invoice = BuildInvoice();
        var options = new ExportOptions { SheetName = "MyInvoice" };
        byte[] result = _exporter.Export(invoice, options);

        using var stream = new MemoryStream(result);
        using var doc = SpreadsheetDocument.Open(stream, false);

        var sheets = doc.WorkbookPart!.Workbook.Descendants<Sheet>().ToList();
        Assert.Single(sheets);
        Assert.Equal("MyInvoice", sheets[0].Name?.Value);
    }

    [Fact]
    public void Export_SimpleTemplate_ProducesOutput()
    {
        var invoice = BuildInvoice();
        var options = new ExportOptions { TemplateName = "Simple" };

        byte[] result = _exporter.Export(invoice, options);

        Assert.True(result.Length > 500);
    }

    [Fact]
    public void Export_CorporateTemplate_ProducesOutput()
    {
        var invoice = BuildInvoice();
        var options = new ExportOptions { TemplateName = "Corporate" };

        byte[] result = _exporter.Export(invoice, options);

        Assert.True(result.Length > 500);
    }

    [Fact]
    public void Export_WorksheetHasMultipleRows()
    {
        var invoice = BuildInvoice();
        byte[] result = _exporter.Export(invoice);

        using var stream = new MemoryStream(result);
        using var doc    = SpreadsheetDocument.Open(stream, false);

        var worksheetPart = doc.WorkbookPart!.WorksheetParts.First();
        var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();

        Assert.True(rows.Count > 5, $"Expected more than 5 rows, got {rows.Count}");
    }

    [Fact]
    public void Export_WorksheetContainsMergedCells()
    {
        var invoice = BuildInvoice();
        byte[] result = _exporter.Export(invoice);

        using var stream = new MemoryStream(result);
        using var doc    = SpreadsheetDocument.Open(stream, false);

        var worksheetPart = doc.WorkbookPart!.WorksheetParts.First();
        var mergeCells = worksheetPart.Worksheet.Descendants<MergeCells>().FirstOrDefault();

        Assert.NotNull(mergeCells);
        Assert.True((uint)mergeCells.Count! > 0);
    }

    [Fact]
    public void Export_InvoiceWithNoNotes_DoesNotThrow()
    {
        var invoice = BuildInvoice();
        invoice.Notes = null;

        var exception = Record.Exception(() => _exporter.Export(invoice));

        Assert.Null(exception);
    }
}
