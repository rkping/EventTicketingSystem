using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Domain.Entities;

public sealed class EventTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tech Conference 2025";
        var description = "Annual tech conference";
        var venue = "Convention Center";
        var eventDate = new DateOnly(2025, 6, 15);
        var eventTime = new TimeOnly(09, 00);
        var totalCapacity = 1000;

        // Act
        var @event = new Event(id, name, description, venue, eventDate, eventTime, totalCapacity);

        // Assert
        @event.Id.Should().Be(id);
        @event.Name.Should().Be(name);
        @event.Description.Should().Be(description);
        @event.Venue.Should().Be(venue);
        @event.EventDate.Should().Be(eventDate);
        @event.EventTime.Should().Be(eventTime);
        @event.TotalCapacity.Should().Be(totalCapacity);
        @event.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        @event.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public void AddPricingTier_ValidTier_AddsPricingTier()
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
        @event.AddPricingTier("VIP", 150m, 100);

        // Assert
        @event.PricingTiers.Should().HaveCount(1);
        @event.PricingTiers.First().Name.Should().Be("VIP");
        @event.PricingTiers.First().Price.Should().Be(150m);
        @event.PricingTiers.First().Capacity.Should().Be(100);
    }

    [Fact]
    public void AddPricingTier_MultipleTiers_AddsAllTiers()
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
        @event.AddPricingTier("VIP", 150m, 100);
        @event.AddPricingTier("Standard", 75m, 500);
        @event.AddPricingTier("Economy", 25m, 400);

        // Assert
        @event.PricingTiers.Should().HaveCount(3);
        @event.PricingTiers.Should().ContainSingle(t => t.Name == "VIP");
        @event.PricingTiers.Should().ContainSingle(t => t.Name == "Standard");
        @event.PricingTiers.Should().ContainSingle(t => t.Name == "Economy");
    }

    [Fact]
    public void AddPricingTier_CapacityExceedsTotalCapacity_ThrowsInvalidOperationException()
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
        @event.AddPricingTier("Standard", 75m, 300);

        // Act
        var action = () => @event.AddPricingTier("Economy", 25m, 150);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Pricing tier capacity exceeds total event capacity.");
    }

    [Fact]
    public void AddPricingTier_InvalidName_ThrowsArgumentException()
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
        var action = () => @event.AddPricingTier("", 150m, 100);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Pricing tier name is required.*");
    }

    [Fact]
    public void AddPricingTier_InvalidCapacity_ThrowsArgumentException()
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
        var action = () => @event.AddPricingTier("VIP", 150m, 0);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Pricing tier capacity must be greater than zero.*");
    }

    [Fact]
    public void AddPricingTier_InvalidPrice_ThrowsArgumentException()
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
        var action = () => @event.AddPricingTier("VIP", -10m, 100);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Pricing tier price cannot be negative.*");
    }

    [Fact]
    public void AddPricingTier_NameWithWhitespace_TrimsName()
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
    }
}
