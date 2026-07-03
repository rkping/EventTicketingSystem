using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Common;
using EventTicketing.Application.Events;
using EventTicketing.Application.Events.Queries.ListEvents;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventTicketing.Tests.Application.Events.Queries;

public sealed class ListEventsQueryHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly ListEventsQueryHandler _handler;

    public ListEventsQueryHandlerTests()
    {
        _handler = new ListEventsQueryHandler(_mockEventRepository.Object);
    }

    [Fact]
    public async Task Handle_EventsExist_ReturnsPagedOrListResponse()
    {
        // Arrange
        var events = new List<EventSummaryResponse>
        {
            new EventSummaryResponse(Guid.NewGuid(), "Event 1", "Venue 1", new DateOnly(2025, 6, 15), new TimeOnly(09, 00), 100, 20, 80),
            new EventSummaryResponse(Guid.NewGuid(), "Event 2", "Venue 2", new DateOnly(2025, 7, 20), new TimeOnly(14, 00), 200, 50, 150)
        };
        var pagedResult = new PagedResult<EventSummaryResponse>(events, 1, 20, 2);

        _mockEventRepository
            .Setup(x => x.ListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new ListEventsQuery(1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_NoEvents_ReturnsEmptyList()
    {
        // Arrange
        var pagedResult = new PagedResult<EventSummaryResponse>(new List<EventSummaryResponse>(), 1, 20, 0);

        _mockEventRepository
            .Setup(x => x.ListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new ListEventsQuery(1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_InvalidPaginationNegativePageNumber_NormalizesToOne()
    {
        // Arrange
        var pagedResult = new PagedResult<EventSummaryResponse>(new List<EventSummaryResponse>(), 1, 20, 0);

        _mockEventRepository
            .Setup(x => x.ListAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new ListEventsQuery(-5, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(x => x.ListAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPaginationPageSizeTooLarge_NormalizesToMaximum()
    {
        // Arrange
        var pagedResult = new PagedResult<EventSummaryResponse>(new List<EventSummaryResponse>(), 1, 100, 0);

        _mockEventRepository
            .Setup(x => x.ListAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new ListEventsQuery(1, 500);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(x => x.ListAsync(1, 100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
