using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Tests.Common.Builders;
using EventTicketing.Tests.Common.Fixtures;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Infrastructure.Repositories;

public sealed class PricingTierRepositoryTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;
    private PricingTierRepository _repository => new(DbContext);
    private EventRepository _eventRepository => new(DbContext);

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task GetAsync_TierExists_ReturnsPricingTier()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAsync(eventId, tierId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("VIP");
        result.Price.Should().Be(150m);
    }

    [Fact]
    public async Task GetAsync_TierDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_TierBelongsToDifferentEvent_ReturnsNull()
    {
        // Arrange
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId1).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var tierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAsync(eventId2, tierId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
