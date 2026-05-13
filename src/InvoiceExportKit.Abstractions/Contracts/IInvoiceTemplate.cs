using InvoiceExportKit.Abstractions.Options;

namespace InvoiceExportKit.Abstractions.Contracts;

/// <summary>
/// A named visual template that supplies render options to the Open XML exporter.
/// Implement this interface to define custom invoice layouts and colour schemes.
/// </summary>
public interface IInvoiceTemplate
{
    /// <summary>Unique name used to look up this template (case-insensitive).</summary>
    string Name { get; }

    /// <summary>Visual options applied during worksheet composition.</summary>
    TemplateRenderOptions RenderOptions { get; }
}
