using InvoiceExportKit.Abstractions.Models;
using InvoiceExportKit.Core.Calculation;
using Xunit;

namespace InvoiceExportKit.Core.Tests.Calculation;

public sealed class InvoiceTotalsCalculatorTests
{
    private readonly InvoiceTotalsCalculator _sut = new();

    private static InvoiceModel InvoiceWith(params InvoiceItemModel[] items) => new()
    {
        InvoiceNumber = "TEST-001",
        IssueDate = DateTime.Today,
        DueDate   = DateTime.Today.AddDays(30),
        Currency  = "USD",
        TaxRate   = 0m,
        Seller    = new ContactModel { Name = "S", Address = new AddressModel { Street = "s", City = "c", Country = "c" } },
        Buyer     = new ContactModel { Name = "B", Address = new AddressModel { Street = "s", City = "c", Country = "c" } },
        Items     = items.ToList()
    };

    [Fact]
    public void SingleItem_NoDiscount_NoTax_CorrectTotal()
    {
        var invoice = InvoiceWith(new InvoiceItemModel
        {
            Description = "Widget",
            Quantity    = 5,
            UnitPrice   = 10.00m,
            Unit        = "pcs"
        });

        var totals = _sut.Calculate(invoice);

        Assert.Equal(50.00m, totals.Subtotal);
        Assert.Equal(0m,     totals.DiscountAmount);
        Assert.Equal(50.00m, totals.TaxableAmount);
        Assert.Equal(0m,     totals.TaxAmount);
        Assert.Equal(50.00m, totals.Total);
    }

    [Fact]
    public void SingleItem_WithDiscount_CorrectLineTotal()
    {
        var item = new InvoiceItemModel
        {
            Description     = "Widget",
            Quantity        = 10,
            UnitPrice       = 100.00m,
            DiscountPercent = 20m,
            Unit            = "pcs"
        };
        var invoice = InvoiceWith(item);

        _sut.Calculate(invoice);

        Assert.Equal(800.00m, item.LineTotal);
    }

    [Fact]
    public void MultipleItems_SumsSubtotalCorrectly()
    {
        var invoice = InvoiceWith(
            new InvoiceItemModel { Description = "A", Quantity = 2, UnitPrice = 50m,  Unit = "pcs" },
            new InvoiceItemModel { Description = "B", Quantity = 3, UnitPrice = 30m,  Unit = "pcs" },
            new InvoiceItemModel { Description = "C", Quantity = 1, UnitPrice = 200m, Unit = "pcs" }
        );

        var totals = _sut.Calculate(invoice);

        Assert.Equal(390.00m, totals.Subtotal);
    }

    [Fact]
    public void TaxRateApplied_AfterDiscount()
    {
        var invoice = InvoiceWith(new InvoiceItemModel
        {
            Description     = "Service",
            Quantity        = 1,
            UnitPrice       = 1000m,
            DiscountPercent = 10m,
            Unit            = "hrs"
        });
        invoice.TaxRate = 21m;

        var totals = _sut.Calculate(invoice);

        Assert.Equal(900.00m,  totals.TaxableAmount);
        Assert.Equal(189.00m,  totals.TaxAmount);
        Assert.Equal(1089.00m, totals.Total);
    }

    [Fact]
    public void ZeroQuantity_LineTotalIsZero()
    {
        var item = new InvoiceItemModel
        {
            Description = "Zero Qty",
            Quantity    = 0,
            UnitPrice   = 99.99m,
            Unit        = "pcs"
        };
        var invoice = InvoiceWith(item);

        _sut.Calculate(invoice);

        Assert.Equal(0m, item.LineTotal);
    }

    [Fact]
    public void Calculate_PopulatesLineTotalsOnItems()
    {
        var items = new[]
        {
            new InvoiceItemModel { Description = "X", Quantity = 4, UnitPrice = 25m, Unit = "pcs" },
            new InvoiceItemModel { Description = "Y", Quantity = 2, UnitPrice = 50m, Unit = "pcs" }
        };
        var invoice = InvoiceWith(items);

        _sut.Calculate(invoice);

        Assert.Equal(100m, items[0].LineTotal);
        Assert.Equal(100m, items[1].LineTotal);
    }

    [Fact]
    public void FullDiscount_LineTotalIsZero()
    {
        var item = new InvoiceItemModel
        {
            Description     = "Free item",
            Quantity        = 1,
            UnitPrice       = 500m,
            DiscountPercent = 100m,
            Unit            = "pcs"
        };
        var invoice = InvoiceWith(item);

        var totals = _sut.Calculate(invoice);

        Assert.Equal(0m, item.LineTotal);
        Assert.Equal(0m, totals.Total);
    }

    [Fact]
    public void ThrowsArgumentNull_WhenInvoiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Calculate(null!));
    }
}
