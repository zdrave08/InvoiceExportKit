namespace InvoiceExportKit.Abstractions.Models;

/// <summary>Calculated financial summary for an invoice.</summary>
public sealed class InvoiceTotalsModel
{
    /// <summary>Sum of all line totals before any invoice-level discount or tax.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Total monetary discount deducted (derived from per-item discounts).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Amount subject to tax after discounts: <c>Subtotal − DiscountAmount</c>.</summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>Tax rate as a percentage (e.g. 20 for 20 %). Zero means tax-exempt.</summary>
    public decimal TaxRate { get; set; }

    /// <summary>Computed tax monetary amount: <c>TaxableAmount × TaxRate / 100</c>.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Final amount due: <c>TaxableAmount + TaxAmount</c>.</summary>
    public decimal Total { get; set; }
}
