namespace InvoiceExportKit.Abstractions.Models;

/// <summary>Root model representing a complete invoice ready for export.</summary>
public sealed class InvoiceModel
{
    /// <summary>Unique invoice identifier shown on the document (e.g. "INV-2024-001").</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>Date the invoice was issued.</summary>
    public DateTime IssueDate { get; set; }

    /// <summary>Payment due date.</summary>
    public DateTime DueDate { get; set; }

    /// <summary>ISO 4217 currency code (e.g. "USD", "EUR").</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Tax rate applied to the taxable amount (e.g. 20 for 20 %). Use 0 for tax-exempt.</summary>
    public decimal TaxRate { get; set; }

    /// <summary>Optional free-text notes or payment instructions printed at the bottom.</summary>
    public string? Notes { get; set; }

    /// <summary>Selling party information.</summary>
    public ContactModel Seller { get; set; } = new();

    /// <summary>Buying party information.</summary>
    public ContactModel Buyer { get; set; } = new();

    /// <summary>Line items on this invoice.</summary>
    public List<InvoiceItemModel> Items { get; set; } = new();

    /// <summary>
    /// Calculated totals. Populated automatically by <c>InvoiceExportService</c>;
    /// you may leave this <c>null</c> when constructing the model.
    /// </summary>
    public InvoiceTotalsModel? Totals { get; set; }
}
