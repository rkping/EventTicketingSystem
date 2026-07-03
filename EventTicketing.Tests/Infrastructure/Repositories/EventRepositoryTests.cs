using EventTicketing.Application.Events;
using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Tests.Common.Builders;
using EventTicketing.Tests.Common.Fixtures;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Infrastructure.Repositories;

public sealed class EventRepositoryTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;
    private EventRepository _repository => new(DbContext);

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task AddAsync_ValidEvent_PersistsEvent()
    {
        // Arrange
        var @event = new EventBuilder().Build();
        @event.AddPricingTier("VIP", 150m, 100);

        // Act
        await _repository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedEvent = await DbContext.Events.FindAsync(@event.Id);
        savedEvent.Should().NotBeNull();
        savedEvent!.Name.Should().Be(@event.Name);
    }

    [Fact]
    public async Task GetDetailsAsync_EventExists_ReturnsEventWithPricingTiers()
    {
        // Arrange
        var @event = new EventBuilder().Build();
        @event.AddPricingTier("VIP", 150m, 100);
        @event.AddPricingTier("Standard", 75m, 500);

        await _repository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDetailsAsync(@event.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(@event.Name);
        result.PricingTiers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDetailsAsync_EventDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetDetailsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAggregateAsync_EventExists_ReturnsEventAggregate()
    {
        // Arrange
        var @event = new EventBuilder().Build();
        @event.AddPricingTier("VIP", 150m, 100);

        await _repository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAggregateAsync(@event.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(@event.Id);
        result.PricingTiers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAggregateAsync_EventDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetAggregateAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_NoEvents_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.ListAsync(1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ListAsync_EventsExist_ReturnsEvents()
    {
        // Arrange
        var event1 = new EventBuilder().WithName("Event 1").Build();
        var event2 = new EventBuilder().WithName("Event 2").Build();

        await _repository.AddAsync(event1, CancellationToken.None);
        await _repository.AddAsync(event2, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync(1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAvailabilityAsync_EventExists_ReturnsAvailability()
    {
        // Arrange
        var @event = new EventBuilder().WithTotalCapacity(1000).Build();
        @event.AddPricingTier("VIP", 150m, 100);

        await _repository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAvailabilityAsync(@event.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableTickets.Should().Be(1000);
        result.SoldTickets.Should().Be(0);
    }

    [Fact]
    public async Task GetTicketSalesReportAsync_EventExists_ReturnsSalesReport()
    {
        // Arrange
        var @event = new EventBuilder().Build();
        @event.AddPricingTier("VIP", 150m, 100);

        await _repository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTicketSalesReportAsync(@event.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.EventName.Should().Be(@event.Name);
    }
}
