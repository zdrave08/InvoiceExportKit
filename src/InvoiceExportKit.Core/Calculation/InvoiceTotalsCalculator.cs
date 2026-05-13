using InvoiceExportKit.Abstractions.Models;

namespace InvoiceExportKit.Core.Calculation;

/// <summary>
/// Calculates line totals and the invoice totals block.
/// All arithmetic is done with <see cref="decimal"/> to avoid floating-point drift.
/// </summary>
public sealed class InvoiceTotalsCalculator
{
    /// <summary>
    /// Calculates every <see cref="InvoiceItemModel.LineTotal"/> in-place
    /// and returns a freshly computed <see cref="InvoiceTotalsModel"/>.
    /// </summary>
    public InvoiceTotalsModel Calculate(InvoiceModel invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        decimal subtotal = 0m;
        decimal discountAmount = 0m;

        foreach (var item in invoice.Items)
        {
            decimal gross = item.Quantity * item.UnitPrice;
            decimal itemDiscount = gross * item.DiscountPercent / 100m;
            item.LineTotal = Math.Round(gross - itemDiscount, 2, MidpointRounding.AwayFromZero);

            subtotal += gross;
            discountAmount += itemDiscount;
        }

        subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        discountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);

        decimal taxableAmount = subtotal - discountAmount;
        decimal taxAmount = Math.Round(taxableAmount * invoice.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
        decimal total = taxableAmount + taxAmount;

        return new InvoiceTotalsModel
        {
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TaxableAmount = taxableAmount,
            TaxRate = invoice.TaxRate,
            TaxAmount = taxAmount,
            Total = total
        };
    }
}
