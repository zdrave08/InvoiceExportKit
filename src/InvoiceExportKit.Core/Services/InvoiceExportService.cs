using InvoiceExportKit.Abstractions.Contracts;
using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;
using InvoiceExportKit.Core.Calculation;
using InvoiceExportKit.Core.Validation;

namespace InvoiceExportKit.Core.Services;

/// <summary>
/// Orchestrates validation, totals calculation, and delegation to a format-specific exporter.
/// This is the primary entry point for consumer code.
/// </summary>
public sealed class InvoiceExportService
{
    private readonly IInvoiceExporter _exporter;
    private readonly IInvoiceValidator _validator;
    private readonly InvoiceTotalsCalculator _calculator;

    /// <param name="exporter">Format-specific exporter (e.g. Excel Open XML).</param>
    /// <param name="validator">Optional custom validator. Defaults to <see cref="InvoiceValidator"/>.</param>
    public InvoiceExportService(IInvoiceExporter exporter, IInvoiceValidator? validator = null)
    {
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
        _validator = validator ?? new InvoiceValidator();
        _calculator = new InvoiceTotalsCalculator();
    }

    /// <summary>
    /// Validates, optionally recalculates totals, then exports <paramref name="invoice"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public byte[] Export(InvoiceModel invoice, ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var validationResult = _validator.Validate(invoice);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors);
            throw new InvalidOperationException($"Invoice validation failed: {errors}");
        }

        options ??= new ExportOptions();

        if (options.RecalculateTotals)
            invoice.Totals = _calculator.Calculate(invoice);

        return _exporter.Export(invoice, options);
    }
}
