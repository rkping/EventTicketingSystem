using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Domain.Entities;

public sealed class PricingTierTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var name = "VIP";
        var price = 150m;
        var capacity = 100;

        // Act
        var tier = new PricingTier(id, eventId, name, price, capacity);

        // Assert
        tier.Id.Should().Be(id);
        tier.EventId.Should().Be(eventId);
        tier.Name.Should().Be(name);
        tier.Price.Should().Be(price);
        tier.Capacity.Should().Be(capacity);
        tier.SoldQuantity.Should().Be(0);
    }

    [Fact]
    public void AvailableQuantity_WhenSoldQuantityIsZero_ReturnsCapacity()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        // Act
        var availableQuantity = tier.AvailableQuantity;

        // Assert
        availableQuantity.Should().Be(100);
    }

    [Fact]
    public void AvailableQuantity_WhenTicketsSold_ReturnsCapacityMinusSold()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        // Act
        tier.MarkSoldForDomainTests(30);
        var availableQuantity = tier.AvailableQuantity;

        // Assert
        availableQuantity.Should().Be(70);
    }

    [Fact]
    public void EnsureCanPurchase_ValidQuantity_DoesNotThrow()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        // Act
        var action = () => tier.EnsureCanPurchase(50);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanPurchase_QuantityExceedsAvailable_ThrowsNotEnoughTicketsException()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);
        tier.MarkSoldForDomainTests(90);

        // Act
        var action = () => tier.EnsureCanPurchase(20);

        // Assert
        action.Should().Throw<NotEnoughTicketsException>()
            .WithMessage("Not enough tickets available.");
    }

    [Fact]
    public void EnsureCanPurchase_ZeroQuantity_ThrowsDomainException()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        // Act
        var action = () => tier.EnsureCanPurchase(0);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void EnsureCanPurchase_ExactRemainingQuantity_DoesNotThrow()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);
        tier.MarkSoldForDomainTests(70);

        // Act
        var action = () => tier.EnsureCanPurchase(30);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsDomainException()
    {
        // Act
        var action = () => new PricingTier(Guid.Empty, Guid.NewGuid(), "VIP", 150m, 100);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Pricing tier id is required.");
    }

    [Fact]
    public void Constructor_EmptyEventId_ThrowsDomainException()
    {
        // Act
        var action = () => new PricingTier(Guid.NewGuid(), Guid.Empty, "VIP", 150m, 100);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Event id is required.");
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsDomainException()
    {
        // Act
        var action = () => new PricingTier(Guid.NewGuid(), Guid.NewGuid(), "", 150m, 100);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Pricing tier name is required.");
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsDomainException()
    {
        // Act
        var action = () => new PricingTier(Guid.NewGuid(), Guid.NewGuid(), "VIP", -10m, 100);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Price cannot be negative.");
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsDomainException()
    {
        // Act
        var action = () => new PricingTier(Guid.NewGuid(), Guid.NewGuid(), "VIP", 150m, 0);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Pricing tier capacity must be greater than zero.");
    }
}
