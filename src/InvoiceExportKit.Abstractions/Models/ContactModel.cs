namespace InvoiceExportKit.Abstractions.Models;

/// <summary>Represents a seller or buyer party on an invoice.</summary>
public sealed class ContactModel
{
    /// <summary>Legal company or person name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Tax registration / VAT identifier.</summary>
    public string? TaxId { get; set; }

    /// <summary>Postal address.</summary>
    public AddressModel Address { get; set; } = new();

    /// <summary>Contact email address.</summary>
    public string? Email { get; set; }

    /// <summary>Contact phone number.</summary>
    public string? Phone { get; set; }
}
