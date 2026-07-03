using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Infrastructure.Services;
using EventTicketing.Tests.Common.Builders;
using EventTicketing.Tests.Common.Fixtures;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Infrastructure.Services;

public sealed class TicketInventoryServiceTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;
    private TicketInventoryService _service => new(DbContext);
    private EventRepository _eventRepository => new(DbContext);

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task TryReserveTicketsAsync_AvailableTickets_ReturnsTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 30, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_AvailableTickets_IncrementsSoldQuantity()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        await _service.TryReserveTicketsAsync(eventId, tierId, 30, CancellationToken.None);

        // Assert
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(30);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_NotEnoughTickets_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 150, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_InvalidEventOrTier_ReturnsFalse()
    {
        // Act
        var result = await _service.TryReserveTicketsAsync(Guid.NewGuid(), Guid.NewGuid(), 10, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_ExactRemainingQuantity_ReturnsTrue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 100, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_ConcurrentRequests_DoesNotOversell()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - 50 parallel tasks attempting to reserve 1 ticket each
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _service.TryReserveTicketsAsync(eventId, tierId, 1, CancellationToken.None))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        var successCount = results.Count(r => r);
        successCount.Should().Be(10, "Only 10 reservations should succeed");

        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(10, "SoldQuantity should equal capacity");
        tier.AvailableQuantity.Should().Be(0, "Remaining availability should be zero");
    }

    [Fact]
    public async Task TryReserveTicketsAsync_SequentialReservations_AggregateCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        await _service.TryReserveTicketsAsync(eventId, tierId, 30, CancellationToken.None);
        await _service.TryReserveTicketsAsync(eventId, tierId, 40, CancellationToken.None);
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 31, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(70);
    }
}
