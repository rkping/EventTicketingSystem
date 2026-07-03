using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Domain.Entities;

public sealed class TicketPurchaseTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var pricingTierId = Guid.NewGuid();
        var buyerName = "John Doe";
        var buyerEmail = "john@example.com";
        var quantity = 2;
        var unitPrice = 150m;
        var purchasedAtUtc = DateTimeOffset.UtcNow;

        // Act
        var purchase = new TicketPurchase(
            id,
            eventId,
            pricingTierId,
            buyerName,
            buyerEmail,
            quantity,
            unitPrice,
            purchasedAtUtc);

        // Assert
        purchase.Id.Should().Be(id);
        purchase.EventId.Should().Be(eventId);
        purchase.PricingTierId.Should().Be(pricingTierId);
        purchase.BuyerName.Should().Be(buyerName);
        purchase.BuyerEmail.Should().Be(buyerEmail);
        purchase.Quantity.Should().Be(quantity);
        purchase.UnitPrice.Should().Be(unitPrice);
        purchase.PurchasedAtUtc.Should().Be(purchasedAtUtc);
    }

    [Fact]
    public void TotalAmount_QuantityAndUnitPrice_ReturnsCorrectAmount()
    {
        // Arrange
        var purchase = new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Act
        var totalAmount = purchase.TotalAmount;

        // Assert
        totalAmount.Should().Be(300m);
    }

    [Fact]
    public void TotalAmount_ComplexMultiplication_CalculatesCorrectly()
    {
        // Arrange
        var purchase = new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            5,
            99.99m,
            DateTimeOffset.UtcNow);

        // Act
        var totalAmount = purchase.TotalAmount;

        // Assert
        totalAmount.Should().Be(499.95m);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Purchase id is required.");
    }

    [Fact]
    public void Constructor_EmptyEventId_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Event id is required.");
    }

    [Fact]
    public void Constructor_EmptyPricingTierId_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "John Doe",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Pricing tier id is required.");
    }

    [Fact]
    public void Constructor_EmptyBuyerName_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Buyer name is required.");
    }

    [Fact]
    public void Constructor_EmptyBuyerEmail_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Buyer email is required.");
    }

    [Fact]
    public void Constructor_InvalidQuantity_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            0,
            150m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void Constructor_NegativeUnitPrice_ThrowsDomainException()
    {
        // Act
        var action = () => new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            2,
            -50m,
            DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Unit price cannot be negative.");
    }

    [Fact]
    public void Constructor_BuyerNameWithWhitespace_TrimsBuyerName()
    {
        // Arrange
        var purchase = new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  John Doe  ",
            "john@example.com",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Act & Assert
        purchase.BuyerName.Should().Be("John Doe");
    }

    [Fact]
    public void Constructor_BuyerEmailWithWhitespace_TrimsBuyerEmail()
    {
        // Arrange
        var purchase = new TicketPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "John Doe",
            "  john@example.com  ",
            2,
            150m,
            DateTimeOffset.UtcNow);

        // Act & Assert
        purchase.BuyerEmail.Should().Be("john@example.com");
    }
}
