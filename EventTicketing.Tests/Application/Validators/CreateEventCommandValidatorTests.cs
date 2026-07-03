using EventTicketing.Application.Events;
using EventTicketing.Application.Events.commands.CreateEvent;
using EventTicketing.Application.Events.Commands.CreateEvent;
using EventTicketing.Tests.Common.Builders;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Application.Validators;

public sealed class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_IsValid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder().Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyName_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithName("")
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_EmptyDescription_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithDescription("")
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_EmptyVenue_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithVenue("")
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Venue");
    }

    [Fact]
    public async Task Validate_TotalCapacityZero_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithTotalCapacity(0)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TotalCapacity");
    }

    [Fact]
    public async Task Validate_TotalCapacityNegative_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithTotalCapacity(-100)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TotalCapacity");
    }

    [Fact]
    public async Task Validate_EmptyPricingTiers_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithPricingTiers()
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTiers");
    }



    [Fact]
    public async Task Validate_PricingTierNameEmpty_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithPricingTiers(
                new PricingTierRequest("", 150m, 100))
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTiers[0].Name");
    }

    [Fact]
    public async Task Validate_PricingTierCapacityZero_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithPricingTiers(
                new PricingTierRequest("VIP", 150m, 0))
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTiers[0].Capacity");
    }

    [Fact]
    public async Task Validate_PricingTierCapacityNegative_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithPricingTiers(
                new PricingTierRequest("VIP", 150m, -50))
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTiers[0].Capacity");
    }

    [Fact]
    public async Task Validate_PricingTierPriceNegative_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithPricingTiers(
                new PricingTierRequest("VIP", -50m, 100))
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTiers[0].Price");
    }

    [Fact]
    public async Task Validate_SumOfTierCapacitiesGreaterThanTotalCapacity_IsInvalid()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithTotalCapacity(500)
            .WithPricingTiers(
                new PricingTierRequest("VIP", 150m, 300),
                new PricingTierRequest("Standard", 75m, 300))
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "");
    }
}
