namespace InvoiceExportKit.Abstractions.Models;

/// <summary>Represents a postal address.</summary>
public sealed class AddressModel
{
    /// <summary>Street line (e.g. "123 Main St, Suite 4").</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>City name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Postal / ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Country name.</summary>
    public string Country { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString() =>
        string.Join(", ", new[] { Street, City, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
