namespace InvoiceExportKit.Abstractions.Models;

/// <summary>Represents a single line item on an invoice.</summary>
public sealed class InvoiceItemModel
{
    /// <summary>Optional product / SKU code.</summary>
    public string? Code { get; set; }

    /// <summary>Human-readable description of the product or service.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Quantity ordered.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure (e.g. "pcs", "hrs", "kg").</summary>
    public string Unit { get; set; } = "pcs";

    /// <summary>Price per single unit before any discount.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount as a percentage (0–100). Zero means no discount.</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>
    /// Pre-calculated line total: <c>Quantity × UnitPrice × (1 − DiscountPercent / 100)</c>.
    /// Populated by <c>InvoiceTotalsCalculator</c> — leave at default when building the model.
    /// </summary>
    public decimal LineTotal { get; set; }
}
