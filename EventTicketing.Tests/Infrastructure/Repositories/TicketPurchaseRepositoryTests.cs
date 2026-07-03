using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Tests.Common.Builders;
using EventTicketing.Tests.Common.Fixtures;
using FluentAssertions;
using Xunit;

namespace EventTicketing.Tests.Infrastructure.Repositories;

public sealed class TicketPurchaseRepositoryTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;
    private TicketPurchaseRepository _repository => new(DbContext);
    private EventRepository _eventRepository => new(DbContext);

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task AddAsync_ValidPurchase_PersistsPurchase()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new EventBuilder().WithId(eventId).Build();
        @event.AddPricingTier("VIP", 150m, 100);
        var pricingTierId = @event.PricingTiers.First().Id;

        await _eventRepository.AddAsync(@event, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var purchase = new TicketPurchaseBuilder()
            .WithEventId(eventId)
            .WithPricingTierId(pricingTierId)
            .Build();

        // Act
        await _repository.AddAsync(purchase, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedPurchase = await DbContext.TicketPurchases.FindAsync(purchase.Id);
        savedPurchase.Should().NotBeNull();
        savedPurchase!.EventId.Should().Be(eventId);
    }
}
