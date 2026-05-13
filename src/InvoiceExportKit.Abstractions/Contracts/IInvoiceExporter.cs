using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Abstractions.Options;

namespace InvoiceExportKit.Abstractions.Contracts;

/// <summary>
/// Converts an <see cref="InvoiceModel"/> into a raw file byte array.
/// Each implementation targets a specific file format (e.g. Excel, PDF).
/// </summary>
public interface IInvoiceExporter
{
    /// <summary>
    /// Exports <paramref name="invoice"/> according to <paramref name="options"/>
    /// and returns the file contents as a byte array.
    /// </summary>
    /// <param name="invoice">The invoice to export. Must not be <c>null</c>.</param>
    /// <param name="options">Export options. Pass <c>null</c> to use defaults.</param>
    /// <returns>Raw file bytes of the produced document.</returns>
    byte[] Export(InvoiceModel invoice, ExportOptions? options = null);
}
