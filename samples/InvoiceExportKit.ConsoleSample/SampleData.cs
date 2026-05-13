using InvoiceExportKit.Abstractions.Models;

namespace InvoiceExportKit.ConsoleSample;

internal static class SampleData
{
    public static InvoiceModel CreateRealisticInvoice() => new()
    {
        InvoiceNumber = "INV-2024-00142",
        IssueDate = new DateTime(2024, 11, 1),
        DueDate   = new DateTime(2024, 11, 30),
        Currency  = "USD",
        TaxRate   = 21m,
        Notes     = "Payment via wire transfer. Reference: INV-2024-00142. "
                  + "Bank: First National Bank, IBAN: US12 3456 7890 1234 5678 90.",

        Seller = new ContactModel
        {
            Name   = "Acme Software Solutions LLC",
            TaxId  = "US-12-3456789",
            Email  = "billing@acmesoftware.com",
            Phone  = "+1 (800) 555-0100",
            Address = new AddressModel
            {
                Street     = "350 Fifth Avenue, Suite 6800",
                City       = "New York",
                PostalCode = "NY 10118",
                Country    = "United States"
            }
        },

        Buyer = new ContactModel
        {
            Name   = "Globex Industrial Corp.",
            TaxId  = "EU-DE-987654321",
            Email  = "accounts@globex-industrial.de",
            Phone  = "+49 89 1234-5678",
            Address = new AddressModel
            {
                Street     = "Maximilianstraße 12",
                City       = "Munich",
                PostalCode = "80539",
                Country    = "Germany"
            }
        },

        Items =
        [
            new InvoiceItemModel
            {
                Code            = "SWL-ENT-2024",
                Description     = "Enterprise Software License — Annual Subscription (50 seats)",
                Quantity        = 1,
                Unit            = "license",
                UnitPrice       = 12_000.00m,
                DiscountPercent = 10m
            },
            new InvoiceItemModel
            {
                Code            = "SVC-IMPL-001",
                Description     = "Professional Implementation Services — Phase 1",
                Quantity        = 40,
                Unit            = "hrs",
                UnitPrice       = 175.00m,
                DiscountPercent = 0m
            },
            new InvoiceItemModel
            {
                Code            = "SVC-TRAIN-GRP",
                Description     = "Group Training Session (up to 15 participants, remote)",
                Quantity        = 3,
                Unit            = "session",
                UnitPrice       = 850.00m,
                DiscountPercent = 5m
            },
            new InvoiceItemModel
            {
                Code            = "SUP-PREM-12M",
                Description     = "Premium Support Package — 12 months (24/7, 4-hour SLA)",
                Quantity        = 1,
                Unit            = "year",
                UnitPrice       = 3_600.00m,
                DiscountPercent = 15m
            },
            new InvoiceItemModel
            {
                Code            = "INFRA-CLOUD-Q4",
                Description     = "Cloud Hosting — Dedicated Environment (Q4 2024)",
                Quantity        = 3,
                Unit            = "month",
                UnitPrice       = 420.00m,
                DiscountPercent = 0m
            }
        ]
    };
}
