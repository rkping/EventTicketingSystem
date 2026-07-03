using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Domain.Entities;

/// <summary>
/// Tests for pricing tier boundary conditions and capacity calculations
/// </summary>
public sealed class PricingTierEdgeCasesTests
{
    [Fact]
    public void EnsureCanPurchase_ExactCapacity_Succeeds()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        tier.MarkSoldForDomainTests(50);

        // Act & Assert - Should not throw
        tier.EnsureCanPurchase(50);
    }

    [Fact]
    public void EnsureCanPurchase_ExceedsAvailable_ThrowsNotEnoughTicketsException()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        tier.MarkSoldForDomainTests(50);

        // Act & Assert
        var action = () => tier.EnsureCanPurchase(51);
        action.Should().Throw<NotEnoughTicketsException>();
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

        // Act & Assert
        var action = () => tier.EnsureCanPurchase(0);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void EnsureCanPurchase_NegativeQuantity_ThrowsDomainException()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        // Act & Assert
        var action = () => tier.EnsureCanPurchase(-5);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AvailableQuantity_CompletelyFull_ReturnsZero()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        tier.MarkSoldForDomainTests(100);

        // Act
        var available = tier.AvailableQuantity;

        // Assert
        available.Should().Be(0);
    }

    [Fact]
    public void AvailableQuantity_AlmostFull_ReturnsOne()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        tier.MarkSoldForDomainTests(99);

        // Act
        var available = tier.AvailableQuantity;

        // Assert
        available.Should().Be(1);
    }

    [Fact]
    public void EnsureCanPurchase_WhenExactlyFull_ThrowsNotEnoughTicketsException()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            100);

        tier.MarkSoldForDomainTests(100);

        // Act & Assert
        var action = () => tier.EnsureCanPurchase(1);
        action.Should().Throw<NotEnoughTicketsException>();
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsDomainException()
    {
        // Act & Assert
        var action = () => new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            150m,
            100);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsDomainException()
    {
        // Act & Assert
        var action = () => new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            -150m,
            100);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsDomainException()
    {
        // Act & Assert
        var action = () => new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            0);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_NegativeCapacity_ThrowsDomainException()
    {
        // Act & Assert
        var action = () => new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIP",
            150m,
            -50);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkSoldForDomainTests_HighPrice_IsAllowed()
    {
        // Arrange
        var tier = new PricingTier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VVIP",
            99999.99m,
            10);

        // Act
        tier.MarkSoldForDomainTests(5);

        // Assert
        tier.SoldQuantity.Should().Be(5);
        tier.Price.Should().Be(99999.99m);
    }
}
