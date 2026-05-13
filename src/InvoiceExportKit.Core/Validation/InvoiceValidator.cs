using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Results;

namespace InvoiceExportKit.Core.Validation;

/// <summary>Default rule-set for invoice validation.</summary>
public sealed class InvoiceValidator : IInvoiceValidator
{
    /// <inheritdoc/>
    public ValidationResult Validate(InvoiceModel invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            result.AddError("InvoiceNumber is required.");

        if (invoice.IssueDate == default)
            result.AddError("IssueDate must be set.");

        if (invoice.DueDate == default)
            result.AddError("DueDate must be set.");

        if (invoice.DueDate < invoice.IssueDate)
            result.AddError("DueDate must not be earlier than IssueDate.");

        if (string.IsNullOrWhiteSpace(invoice.Currency))
            result.AddError("Currency is required.");

        if (invoice.TaxRate < 0 || invoice.TaxRate > 100)
            result.AddError("TaxRate must be between 0 and 100.");

        ValidateContact(invoice.Seller, "Seller", result);
        ValidateContact(invoice.Buyer, "Buyer", result);

        if (invoice.Items is null || invoice.Items.Count == 0)
        {
            result.AddError("At least one invoice item is required.");
        }
        else
        {
            for (int i = 0; i < invoice.Items.Count; i++)
                ValidateItem(invoice.Items[i], i + 1, result);
        }

        return result;
    }

    private static void ValidateContact(ContactModel contact, string role, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(contact.Name))
            result.AddError($"{role}.Name is required.");

        if (string.IsNullOrWhiteSpace(contact.Address?.Street))
            result.AddError($"{role}.Address.Street is required.");

        if (string.IsNullOrWhiteSpace(contact.Address?.City))
            result.AddError($"{role}.Address.City is required.");

        if (string.IsNullOrWhiteSpace(contact.Address?.Country))
            result.AddError($"{role}.Address.Country is required.");
    }

    private static void ValidateItem(InvoiceItemModel item, int lineNumber, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(item.Description))
            result.AddError($"Item #{lineNumber}: Description is required.");

        if (item.Quantity <= 0)
            result.AddError($"Item #{lineNumber}: Quantity must be greater than zero.");

        if (item.UnitPrice < 0)
            result.AddError($"Item #{lineNumber}: UnitPrice must not be negative.");

        if (item.DiscountPercent < 0 || item.DiscountPercent > 100)
            result.AddError($"Item #{lineNumber}: DiscountPercent must be between 0 and 100.");
    }
}
