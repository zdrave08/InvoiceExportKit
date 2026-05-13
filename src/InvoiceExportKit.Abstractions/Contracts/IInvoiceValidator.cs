using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Results;

namespace InvoiceExportKit.Abstractions.Contracts;

/// <summary>Validates an <see cref="InvoiceModel"/> before export.</summary>
public interface IInvoiceValidator
{
    /// <summary>
    /// Validates <paramref name="invoice"/> and returns a <see cref="ValidationResult"/>
    /// containing all detected errors.
    /// </summary>
    ValidationResult Validate(InvoiceModel invoice);
}
