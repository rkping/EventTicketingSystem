using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Domain.Entities;

/// <summary>
/// Tests for edge cases and boundary conditions in Event aggregate
/// </summary>
public sealed class EventEdgeCasesTests
{
    [Fact]
    public void AddPricingTier_SamePriceAndCapacityDifferentName_Succeeds()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        @event.AddPricingTier("VIP", 150m, 100);

        // Act - Adding with different name should succeed even if same price
        @event.AddPricingTier("Premium", 150m, 50);

        // Assert
        @event.PricingTiers.Should().HaveCount(2);
        @event.PricingTiers.Should().ContainSingle(t => t.Name == "VIP");
        @event.PricingTiers.Should().ContainSingle(t => t.Name == "Premium");
    }

    [Fact]
    public void AddPricingTier_NameWithWhitespace_TrimsAndAdds()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        // Act
        @event.AddPricingTier("  VIP  ", 150m, 100);

        // Assert
        @event.PricingTiers.First().Name.Should().Be("VIP");
        @event.PricingTiers.First().Name.Should().NotContain(" ");
    }

    [Fact]
    public void AddPricingTier_CapacityAtExactBoundary_Succeeds()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        // Act
        @event.AddPricingTier("VIP", 150m, 600);
        @event.AddPricingTier("Standard", 75m, 400);

        // Assert
        @event.PricingTiers.Sum(t => t.Capacity).Should().Be(1000);
    }

    [Fact]
    public void AddPricingTier_OneTicketOver_ThrowsInvalidOperationException()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        @event.AddPricingTier("VIP", 150m, 600);
        @event.AddPricingTier("Standard", 75m, 400);

        // Act & Assert
        var action = () => @event.AddPricingTier("Economy", 25m, 1);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*exceeds total event capacity*");
    }

    [Fact]
    public void Update_ReduceCapacityBelowAllocated_ThrowsInvalidOperationException()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        @event.AddPricingTier("VIP", 150m, 600);
        @event.AddPricingTier("Standard", 75m, 400);

        // Act & Assert
        var action = () => @event.Update(
            "Conference",
            "New Description",
            "New Venue",
            new DateOnly(2025, 7, 20),
            new TimeOnly(14, 00),
            500,  // Reduced below allocated 1000
            DateTimeOffset.UtcNow);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be less than the total quantity allocated*");
    }

    [Fact]
    public void Update_ReduceCapacityToExactAllocated_Succeeds()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        @event.AddPricingTier("VIP", 150m, 600);
        @event.AddPricingTier("Standard", 75m, 400);

        // Act
        @event.Update(
            "Conference Updated",
            "New Description",
            "New Venue",
            new DateOnly(2025, 7, 20),
            new TimeOnly(14, 00),
            1000,  // Exactly equal to allocated
            DateTimeOffset.UtcNow);

        // Assert
        @event.Name.Should().Be("Conference Updated");
        @event.TotalCapacity.Should().Be(1000);
    }

    [Fact]
    public void Update_IncreaseCapacity_Succeeds()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        @event.AddPricingTier("VIP", 150m, 600);
        @event.AddPricingTier("Standard", 75m, 400);

        // Act
        @event.Update(
            "Conference Updated",
            "New Description",
            "New Venue",
            new DateOnly(2025, 7, 20),
            new TimeOnly(14, 00),
            2000,  // Increased
            DateTimeOffset.UtcNow);

        // Assert
        @event.TotalCapacity.Should().Be(2000);
        @event.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Update_TrimWhitespaceFromProperties_NormalizesData()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        // Act
        @event.Update(
            "  Conference Updated  ",
            "  New Description  ",
            "  New Venue  ",
            new DateOnly(2025, 7, 20),
            new TimeOnly(14, 00),
            1000,
            DateTimeOffset.UtcNow);

        // Assert
        @event.Name.Should().Be("Conference Updated");
        @event.Description.Should().Be("New Description");
        @event.Venue.Should().Be("New Venue");
    }

    [Fact]
    public void AddPricingTier_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        // Act & Assert
        var action = () => @event.AddPricingTier("VIP", -50m, 100);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddPricingTier_ZeroPrice_IsAllowed()
    {
        // Arrange
        var @event = new Event(
            Guid.NewGuid(),
            "Conference",
            "Description",
            "Venue",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000);

        // Act
        @event.AddPricingTier("Free", 0m, 100);

        // Assert
        @event.PricingTiers.First().Price.Should().Be(0m);
    }
}
