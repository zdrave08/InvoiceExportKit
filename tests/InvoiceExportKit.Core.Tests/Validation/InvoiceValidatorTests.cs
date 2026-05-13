using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Core.Validation;
using Xunit;

namespace InvoiceExportKit.Core.Tests.Validation;

public sealed class InvoiceValidatorTests
{
    private readonly InvoiceValidator _sut = new();

    private static InvoiceModel ValidInvoice() => new()
    {
        InvoiceNumber = "INV-001",
        IssueDate     = new DateTime(2024, 1, 1),
        DueDate       = new DateTime(2024, 1, 31),
        Currency      = "USD",
        TaxRate       = 0m,
        Seller = new ContactModel
        {
            Name = "Seller Co",
            Address = new AddressModel { Street = "1 Main St", City = "NYC", Country = "US" }
        },
        Buyer = new ContactModel
        {
            Name = "Buyer Ltd",
            Address = new AddressModel { Street = "2 Main St", City = "LA", Country = "US" }
        },
        Items =
        [
            new InvoiceItemModel { Description = "Item", Quantity = 1, UnitPrice = 100m, Unit = "pcs" }
        ]
    };

    [Fact]
    public void ValidInvoice_ReturnsIsValid()
    {
        var result = _sut.Validate(ValidInvoice());
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MissingInvoiceNumber_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.InvoiceNumber = "";

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("InvoiceNumber"));
    }

    [Fact]
    public void DueDateBeforeIssueDate_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.DueDate = invoice.IssueDate.AddDays(-1);

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("DueDate"));
    }

    [Fact]
    public void EmptyItems_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.Items.Clear();

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("item"));
    }

    [Fact]
    public void NegativeUnitPrice_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.Items[0].UnitPrice = -1m;

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("UnitPrice"));
    }

    [Fact]
    public void DiscountOver100_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.Items[0].DiscountPercent = 101m;

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("DiscountPercent"));
    }

    [Fact]
    public void MissingSellerName_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.Seller.Name = "";

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Seller") && e.Contains("Name"));
    }

    [Fact]
    public void MissingBuyerCity_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.Buyer.Address.City = "";

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Buyer") && e.Contains("City"));
    }

    [Fact]
    public void NegativeTaxRate_AddsError()
    {
        var invoice = ValidInvoice();
        invoice.TaxRate = -5m;

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("TaxRate"));
    }

    [Fact]
    public void MultipleErrors_AllCollected()
    {
        var invoice = ValidInvoice();
        invoice.InvoiceNumber    = "";
        invoice.Seller.Name      = "";
        invoice.Items[0].Quantity = 0;

        var result = _sut.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }

    [Fact]
    public void ThrowsArgumentNull_WhenInvoiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(null!));
    }
}
