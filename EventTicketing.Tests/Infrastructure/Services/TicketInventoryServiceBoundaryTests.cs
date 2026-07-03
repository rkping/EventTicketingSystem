using EventTicketing.Application.Events;
using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Infrastructure.Services;
using EventTicketing.Tests.Common.Builders;
using EventTicketing.Tests.Common.Fixtures;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Infrastructure.Services;

/// <summary>
/// Advanced edge case tests for ticket inventory service focusing on boundary conditions
/// </summary>
public sealed class TicketInventoryServiceBoundaryTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;
    private TicketInventoryService _service => new(DbContext);
    private EventRepository _eventRepository => new(DbContext);

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task TryReserveTicketsAsync_ExactSoldOutCondition_ReturnsTrue()
    {
        // Arrange - Tier with exactly 10 tickets
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Reserve exactly the capacity
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 10, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(10);
        tier.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_OneOverCapacity_ReturnsFalse()
    {
        // Arrange - Tier with exactly 10 tickets
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Try to reserve 11 tickets
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 11, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(0); // No tickets sold
    }

    [Fact]
    public async Task TryReserveTicketsAsync_ReserveOne_Succeeds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Premium", 200m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(1);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_MultipleReservationsApproachSoldOut_LastOneSucceeds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Sequential reservations
        var result1 = await _service.TryReserveTicketsAsync(eventId, tierId, 5, CancellationToken.None);
        var result2 = await _service.TryReserveTicketsAsync(eventId, tierId, 4, CancellationToken.None);
        var result3 = await _service.TryReserveTicketsAsync(eventId, tierId, 1, CancellationToken.None); // Last one

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(10);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_AfterSoldOut_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Fill capacity
        await _service.TryReserveTicketsAsync(eventId, tierId, 10, CancellationToken.None);
        
        // Try to reserve after sold out
        var resultAfterSoldOut = await _service.TryReserveTicketsAsync(eventId, tierId, 1, CancellationToken.None);

        // Assert
        resultAfterSoldOut.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_InvalidTierId_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Use non-existent tier ID
        var result = await _service.TryReserveTicketsAsync(eventId, Guid.NewGuid(), 5, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_InvalidEventId_ReturnsFalse()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act - Use wrong event ID with correct tier ID
        var result = await _service.TryReserveTicketsAsync(Guid.NewGuid(), tierId, 5, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryReserveTicketsAsync_LargeCapacity_Succeeds()
    {
        // Arrange - Event with large capacity
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).WithTotalCapacity(50000).Build();
        @event.AddPricingTier("General Admission", 25m, 50000);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _service.TryReserveTicketsAsync(eventId, tierId, 5000, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var tier = await DbContext.PricingTiers.FindAsync(tierId);
        tier!.SoldQuantity.Should().Be(5000);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_VersionIncrements_OnSuccess()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var tierBefore = await DbContext.PricingTiers.FindAsync(tierId);
        var versionBefore = tierBefore!.Version;

        // Act
        await _service.TryReserveTicketsAsync(eventId, tierId, 10, CancellationToken.None);

        // Assert
        var tierAfter = await DbContext.PricingTiers.FindAsync(tierId);
        tierAfter!.Version.Should().Be(versionBefore + 1);
    }

    [Fact]
    public async Task TryReserveTicketsAsync_VersionNotIncrements_OnFailure()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("Standard", 75m, 10);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var tierBefore = await DbContext.PricingTiers.FindAsync(tierId);
        var versionBefore = tierBefore!.Version;

        // Act - Try to reserve more than available (fails)
        await _service.TryReserveTicketsAsync(eventId, tierId, 20, CancellationToken.None);

        // Assert
        var tierAfter = await DbContext.PricingTiers.FindAsync(tierId);
        tierAfter!.Version.Should().Be(versionBefore); // Version unchanged on failure
    }
}
