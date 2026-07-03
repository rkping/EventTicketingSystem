using EventTicketing.Application;
using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Tickets.Commands.PurchaseTickets;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using EventTicketing.Tests.Common.Builders;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace EventTicketing.Tests.Application.Tickets.Commands;

public sealed class PurchaseTicketsCommandHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly Mock<IPricingTierRepository> _mockPricingTierRepository = new();
    private readonly Mock<ITicketPurchaseRepository> _mockPurchaseRepository = new();
    private readonly Mock<ITicketInventoryService> _mockInventoryService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly PurchaseTicketsCommandValidator _validator = new();
    private readonly PurchaseTicketsCommandHandler _handler;

    public PurchaseTicketsCommandHandlerTests()
    {
        _handler = new PurchaseTicketsCommandHandler(
            _mockEventRepository.Object,
            _mockPricingTierRepository.Object,
            _mockPurchaseRepository.Object,
            _mockInventoryService.Object,
            _mockUnitOfWork.Object,
            _validator);
    }



    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new PurchaseTicketsCommandBuilder()
            .WithBuyerEmail("invalid-email")
            .Build();

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
        _mockEventRepository.Verify(
            x => x.GetAggregateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var command = new PurchaseTicketsCommandBuilder().WithEventId(eventId).Build();

        _mockEventRepository
            .Setup(x => x.GetAggregateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Event));

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<PurchaseTicketResponse>>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task<PurchaseTicketResponse>> operation, CancellationToken ct) => await operation(ct))
            .Returns<Func<CancellationToken, Task<PurchaseTicketResponse>>, CancellationToken>((op, ct) => op(ct));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Event not found.");
    }

    [Fact]
    public async Task Handle_PricingTierNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var pricingTierId = Guid.NewGuid();
        var command = new PurchaseTicketsCommandBuilder()
            .WithEventId(eventId)
            .WithPricingTierId(pricingTierId)
            .Build();

        var eventEntity = new EventBuilder().WithId(eventId).Build();

        _mockEventRepository
            .Setup(x => x.GetAggregateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _mockPricingTierRepository
            .Setup(x => x.GetAsync(eventId, pricingTierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PricingTier?)null);

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<PurchaseTicketResponse>>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task<PurchaseTicketResponse>> operation, CancellationToken ct) => await operation(ct))
            .Returns<Func<CancellationToken, Task<PurchaseTicketResponse>>, CancellationToken>((op, ct) => op(ct));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Pricing tier not found.");
    }

    [Fact]
    public async Task Handle_NotEnoughTickets_ThrowsConflictException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var pricingTierId = Guid.NewGuid();
        var command = new PurchaseTicketsCommandBuilder()
            .WithEventId(eventId)
            .WithPricingTierId(pricingTierId)
            .Build();

        var eventEntity = new EventBuilder().WithId(eventId).Build();
        var tier = new PricingTierBuilder().WithEventId(eventId).WithId(pricingTierId).Build();

        _mockEventRepository
            .Setup(x => x.GetAggregateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _mockPricingTierRepository
            .Setup(x => x.GetAsync(eventId, pricingTierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tier);

        _mockInventoryService
            .Setup(x => x.TryReserveTicketsAsync(eventId, pricingTierId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<PurchaseTicketResponse>>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task<PurchaseTicketResponse>> operation, CancellationToken ct) => await operation(ct))
            .Returns<Func<CancellationToken, Task<PurchaseTicketResponse>>, CancellationToken>((op, ct) => op(ct));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Not enough tickets available.");
    }

    [Fact]
    public async Task Handle_ValidPurchase_ReturnsPurchaseTicketResponse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var pricingTierId = Guid.NewGuid();
        var command = new PurchaseTicketsCommandBuilder()
            .WithEventId(eventId)
            .WithPricingTierId(pricingTierId)
            .WithQuantity(3)
            .Build();

        var eventEntity = new EventBuilder().WithId(eventId).Build();
        eventEntity.AddPricingTier("VIP", 150m, 100);
        var tier = new PricingTierBuilder().WithEventId(eventId).WithId(pricingTierId).WithPrice(150m).Build();

        _mockEventRepository
            .Setup(x => x.GetAggregateAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _mockPricingTierRepository
            .Setup(x => x.GetAsync(eventId, pricingTierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tier);

        _mockInventoryService
            .Setup(x => x.TryReserveTicketsAsync(eventId, pricingTierId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<PurchaseTicketResponse>>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task<PurchaseTicketResponse>> operation, CancellationToken ct) => await operation(ct))
            .Returns<Func<CancellationToken, Task<PurchaseTicketResponse>>, CancellationToken>((op, ct) => op(ct));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.PricingTierId.Should().Be(pricingTierId);
        result.Quantity.Should().Be(3);
        result.UnitPrice.Should().Be(150m);
        result.TotalAmount.Should().Be(450m);
    }
}
