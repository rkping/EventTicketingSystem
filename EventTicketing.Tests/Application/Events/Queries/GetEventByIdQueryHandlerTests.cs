using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events;
using EventTicketing.Application.Events.Queries.GetEventById;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventTicketing.Tests.Application.Events.Queries;

public sealed class GetEventByIdQueryHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly GetEventByIdQueryHandler _handler;

    public GetEventByIdQueryHandlerTests()
    {
        _handler = new GetEventByIdQueryHandler(_mockEventRepository.Object);
    }

    [Fact]
    public async Task Handle_EventExists_ReturnsEventDetailsResponse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var expectedEvent = new EventDetailsResponse(
            eventId,
            "Tech Conference",
            "Annual tech conference",
            "Convention Center",
            new DateOnly(2025, 6, 15),
            new TimeOnly(09, 00),
            1000,
            0,
            1000,
            new List<PricingTierResponse>());

        _mockEventRepository
            .Setup(x => x.GetDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEvent);

        var query = new GetEventByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedEvent);
        _mockEventRepository.Verify(
            x => x.GetDetailsAsync(eventId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository
            .Setup(x => x.GetDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDetailsResponse?)null);

        var query = new GetEventByIdQuery(eventId);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Event not found.");
    }
}
