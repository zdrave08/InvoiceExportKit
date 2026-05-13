using InvoiceExportKit.ConsoleSample;
using InvoiceExportKit.Core.Services;
using InvoiceExportKit.Excel.OpenXml.Rendering;
using InvoiceExportKit.Abstractions.Options;

Console.WriteLine("InvoiceExportKit — Console Sample");
Console.WriteLine("==================================");

var invoice = SampleData.CreateRealisticInvoice();
var exporter = new ExcelInvoiceExporter();
var service  = new InvoiceExportService(exporter);

string outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);

foreach (string templateName in new[] { "Simple", "Corporate" })
{
    var options = new ExportOptions
    {
        TemplateName     = templateName,
        RecalculateTotals = true,
        SheetName        = "Invoice"
    };

    byte[] bytes = service.Export(invoice, options);

    string filePath = Path.Combine(outputDir, $"invoice_{templateName.ToLower()}.xlsx");
    File.WriteAllBytes(filePath, bytes);

    Console.WriteLine($"[OK] {templateName} template → {filePath}  ({bytes.Length:N0} bytes)");
}

Console.WriteLine();
Console.WriteLine("Done. Open the files in Excel to review the output.");
