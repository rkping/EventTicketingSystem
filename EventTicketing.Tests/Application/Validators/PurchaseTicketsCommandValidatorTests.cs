using EventTicketing.Application.Tickets.Commands.PurchaseTickets;
using EventTicketing.Tests.Common.Builders;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Application.Validators;

public sealed class PurchaseTicketsCommandValidatorTests
{
    private readonly PurchaseTicketsCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_IsValid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder().Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyEventId_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithEventId(Guid.Empty)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "EventId");
    }

    [Fact]
    public async Task Validate_EmptyPricingTierId_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithPricingTierId(Guid.Empty)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PricingTierId");
    }

    [Fact]
    public async Task Validate_EmptyBuyerName_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithBuyerName("")
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "BuyerName");
    }

    [Fact]
    public async Task Validate_InvalidBuyerEmail_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithBuyerEmail("not-an-email")
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "BuyerEmail");
    }

    [Fact]
    public async Task Validate_QuantityZero_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithQuantity(0)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public async Task Validate_QuantityNegative_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithQuantity(-5)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public async Task Validate_QuantityTooLarge_IsInvalid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithQuantity(11)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public async Task Validate_QuantityAtMaximum_IsValid()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithQuantity(10)
            .Build();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
