using EventTicketing.Domain.Entities;

namespace EventTicketing.Application.Abstractions;

public interface IPricingTierRepository
{
    Task<PricingTier?> GetAsync(Guid eventId, Guid pricingTierId, CancellationToken cancellationToken);
}
