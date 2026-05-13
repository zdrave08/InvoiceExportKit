using InvoiceExportKit.Abstractions.Contracts;

namespace InvoiceExportKit.Templates;

/// <summary>
/// Resolves an <see cref="IInvoiceTemplate"/> by name from the built-in template catalogue.
/// </summary>
public static class TemplateRegistry
{
    private static readonly Dictionary<string, IInvoiceTemplate> _templates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Simple"] = new SimpleInvoiceTemplate(),
            ["Corporate"] = new CorporateInvoiceTemplate()
        };

    /// <summary>
    /// Returns the template matching <paramref name="name"/>, falling back to
    /// <see cref="SimpleInvoiceTemplate"/> when the name is not found.
    /// </summary>
    public static IInvoiceTemplate Resolve(string? name)
    {
        if (name is not null && _templates.TryGetValue(name, out var template))
            return template;

        return _templates["Simple"];
    }

    /// <summary>All registered template names.</summary>
    public static IEnumerable<string> AvailableNames => _templates.Keys;
}
