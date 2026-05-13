# InvoiceExportKit

A lightweight, open-source .NET 8 library for exporting professional invoice documents to Excel (`.xlsx`) using the [Open XML SDK](https://github.com/dotnet/Open-XML-SDK). Designed as a reusable, NuGet-friendly toolkit with clean separation of concerns.

---

## Features

- Export invoices to `.xlsx` with full Open XML SDK support
- Seller and buyer information blocks (name, address, tax ID, email, phone)
- Invoice metadata: number, issue date, due date, currency, notes
- Line items with code, description, quantity, unit, unit price, discount %, and totals
- Totals section: subtotal, discount, tax, and final total
- Visual formatting: merged cells, custom column widths, borders, bold headers, number/date formatting, horizontal alignment
- Two built-in templates: **Simple** (clean blue) and **Corporate** (dark header, row banding)
- Pluggable template system — implement `IInvoiceTemplate` and pass it in
- Validation layer with descriptive error messages
- Auto-calculation of line totals and invoice totals
- ASP.NET Core Web API sample with `POST /api/invoices/export/excel`
- Console sample generating both templates to an output folder

---

## Project Structure

```
InvoiceExportKit/
├── src/
│   ├── InvoiceExportKit.Abstractions      # Models, contracts, options (no dependencies)
│   ├── InvoiceExportKit.Core              # Validation + totals calculation (→ Abstractions)
│   ├── InvoiceExportKit.Excel.OpenXml     # Open XML rendering (→ Abstractions, Templates)
│   └── InvoiceExportKit.Templates         # Built-in templates: Simple & Corporate (→ Abstractions)
├── samples/
│   ├── InvoiceExportKit.ConsoleSample     # CLI sample: writes .xlsx files to /output
│   └── InvoiceExportKit.WebApiSample      # ASP.NET Core API sample
└── tests/
    ├── InvoiceExportKit.Core.Tests        # Calculation & validation unit tests
    └── InvoiceExportKit.Excel.OpenXml.Tests # Export integration tests
```

### Dependency Graph

```
Abstractions  ←  Core
Abstractions  ←  Templates
Abstractions  ←  Excel.OpenXml  ←  Templates
```

Business logic (`Core`) has **zero** dependency on the rendering layer (`Excel.OpenXml`).

---

## Quick Start

### 1. Build the invoice model

```csharp
var invoice = new InvoiceModel
{
    InvoiceNumber = "INV-2024-001",
    IssueDate     = new DateTime(2024, 11, 1),
    DueDate       = new DateTime(2024, 11, 30),
    Currency      = "USD",
    TaxRate       = 21m,
    Notes         = "Payment within 30 days. Thank you for your business.",

    Seller = new ContactModel
    {
        Name    = "Acme Software LLC",
        TaxId   = "US-12-3456789",
        Email   = "billing@acme.com",
        Address = new AddressModel
        {
            Street = "350 Fifth Avenue", City = "New York",
            PostalCode = "NY 10118", Country = "United States"
        }
    },

    Buyer = new ContactModel
    {
        Name    = "Globex Corp.",
        Address = new AddressModel
        {
            Street = "742 Evergreen Terrace", City = "Springfield",
            PostalCode = "62701", Country = "United States"
        }
    },

    Items =
    [
        new InvoiceItemModel
        {
            Code = "LIC-ENT", Description = "Enterprise License (annual)",
            Quantity = 1, Unit = "license", UnitPrice = 9_999.00m
        },
        new InvoiceItemModel
        {
            Code = "SVC-IMPL", Description = "Implementation Services",
            Quantity = 20, Unit = "hrs", UnitPrice = 150.00m, DiscountPercent = 10m
        }
    ]
};
```

### 2. Export

```csharp
var exporter = new ExcelInvoiceExporter();
var service  = new InvoiceExportService(exporter);

byte[] bytes = service.Export(invoice, new ExportOptions
{
    TemplateName      = "Corporate",  // or "Simple"
    RecalculateTotals = true,
    SheetName         = "Invoice"
});

File.WriteAllBytes("invoice.xlsx", bytes);
```

### 3. ASP.NET Core DI

```csharp
builder.Services.AddSingleton<IInvoiceExporter, ExcelInvoiceExporter>();
builder.Services.AddSingleton<IInvoiceValidator, InvoiceValidator>();
builder.Services.AddScoped<InvoiceExportService>();
```

Then `POST /api/invoices/export/excel` with the `ExportRequest` JSON body (see `InvoicesController`).

---

## Templates

| Name        | Header      | Row banding | Title size |
|-------------|-------------|-------------|------------|
| `Simple`    | Steel blue  | None        | 18 pt      |
| `Corporate` | Dark navy   | Light blue  | 20 pt      |

### Custom template

```csharp
public class BrandedTemplate : IInvoiceTemplate
{
    public string Name => "Branded";
    public TemplateRenderOptions RenderOptions { get; } = new()
    {
        HeaderBackgroundColor = "C00000",
        HeaderFontColor       = "FFFFFF",
        FontName              = "Arial",
        TitleFontSize         = 22,
        ColumnWidths          = new() { [3] = 42 }
    };
}

var exporter = new ExcelInvoiceExporter(customTemplates: [new BrandedTemplate()]);
```

---

## Running Tests

```bash
dotnet test
```

---

## Running the Console Sample

```bash
cd samples/InvoiceExportKit.ConsoleSample
dotnet run
# Output files written to: bin/Debug/net8.0/output/
```

---

## Running the Web API Sample

```bash
cd samples/InvoiceExportKit.WebApiSample
dotnet run
# Swagger UI available at: https://localhost:5001/swagger
```

**Example cURL request:**

```bash
curl -X POST https://localhost:5001/api/invoices/export/excel \
  -H "Content-Type: application/json" \
  -d '{
    "invoice": {
      "invoiceNumber": "INV-001",
      "issueDate": "2024-11-01",
      "dueDate": "2024-11-30",
      "currency": "USD",
      "taxRate": 20,
      "seller": { "name": "Seller Co", "address": { "street": "1 Main", "city": "NYC", "postalCode": "10001", "country": "US" } },
      "buyer":  { "name": "Buyer Inc", "address": { "street": "2 Main", "city": "LA",  "postalCode": "90001", "country": "US" } },
      "items": [{ "description": "Consulting", "quantity": 5, "unit": "hrs", "unitPrice": 200 }]
    },
    "options": { "templateName": "Corporate" }
  }' \
  --output invoice.xlsx
```

---

## Roadmap

- [ ] PDF export via a pluggable `IPdfInvoiceExporter` (QuestPDF / iText)
- [ ] Multiple currency symbol rendering in cells (not just ISO code)
- [ ] Logo / image embedding support for Corporate template
- [ ] Localization support (date formats, decimal separators) via `CultureInfo`
- [ ] Async export API (`ExportAsync`) for streaming large workbooks
- [ ] IBAN / payment-details section in the template
- [ ] Additional built-in templates: `Minimal`, `A4Formal`
- [ ] NuGet packages published to nuget.org
- [ ] GitHub Actions CI pipeline (build + test)

---

## License

MIT — see [LICENSE](LICENSE) for details.
