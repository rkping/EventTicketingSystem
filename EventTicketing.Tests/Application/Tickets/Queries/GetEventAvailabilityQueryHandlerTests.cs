using EventTicketing.Application;
using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Tickets.Queries.GetEventAvailability;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Moq;
using TicketingSystem.Application.Tickets.Queries.GetEventAvailability;
using Xunit;

namespace EventTicketing.Tests.Application.Tickets.Queries;

public sealed class GetEventAvailabilityQueryHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly GetEventAvailabilityQueryHandler _handler;

    public GetEventAvailabilityQueryHandlerTests()
    {
        _handler = new GetEventAvailabilityQueryHandler(_mockEventRepository.Object);
    }

    [Fact]
    public async Task Handle_EventExists_ReturnsAvailability()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var availability = new AvailabilityResponse(
            eventId,
            1000,
            200,
            800,
            new List<TierAvailabilityResponse>
            {
                new TierAvailabilityResponse(Guid.NewGuid(), "VIP", 100, 30, 70),
                new TierAvailabilityResponse(Guid.NewGuid(), "Standard", 500, 120, 380),
                new TierAvailabilityResponse(Guid.NewGuid(), "Economy", 400, 50, 350)
            });

        _mockEventRepository
            .Setup(x => x.GetAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        var query = new GetEventAvailabilityQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(availability);
        result.AvailableTickets.Should().Be(800);
    }

    [Fact]
    public async Task Handle_EventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository
            .Setup(x => x.GetAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AvailabilityResponse?)null);

        var query = new GetEventAvailabilityQuery(eventId);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SoldTicketsExist_ReturnsCorrectAvailableTickets()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var availability = new AvailabilityResponse(
            eventId,
            1000,
            750,
            250,
            new List<TierAvailabilityResponse>());

        _mockEventRepository
            .Setup(x => x.GetAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        var query = new GetEventAvailabilityQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailableTickets.Should().Be(250);
        result.SoldTickets.Should().Be(750);
    }

    [Fact]
    public async Task Handle_NoSoldTickets_ReturnsFullAvailability()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var availability = new AvailabilityResponse(
            eventId,
            1000,
            0,
            1000,
            new List<TierAvailabilityResponse>());

        _mockEventRepository
            .Setup(x => x.GetAvailabilityAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        var query = new GetEventAvailabilityQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailableTickets.Should().Be(1000);
        result.SoldTickets.Should().Be(0);
    }
}
