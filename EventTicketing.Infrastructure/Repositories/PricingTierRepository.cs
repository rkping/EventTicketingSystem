using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Entities;
using EventTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Repositories;

public sealed class PricingTierRepository : IPricingTierRepository
{
    private readonly TicketingDbContext _dbContext;
    public PricingTierRepository(TicketingDbContext dbContext) => _dbContext = dbContext;

    public async Task<PricingTier?> GetAsync(Guid eventId, Guid pricingTierId, CancellationToken cancellationToken) =>
        await _dbContext.PricingTiers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == pricingTierId && x.EventId == eventId, cancellationToken);
}
