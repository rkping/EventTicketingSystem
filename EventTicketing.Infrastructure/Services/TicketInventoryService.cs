using EventTicketing.Application.Abstractions;
using EventTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Services;

public sealed class TicketInventoryService : ITicketInventoryService
{
    private readonly TicketingDbContext _dbContext;
    public TicketInventoryService(TicketingDbContext dbContext) => _dbContext = dbContext;

    public async Task<bool> TryReserveTicketsAsync(Guid eventId, Guid pricingTierId, int quantity, CancellationToken cancellationToken)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE PricingTiers
            SET SoldQuantity = SoldQuantity + {quantity},
                Version = Version + 1
            WHERE Id = {pricingTierId}
              AND EventId = {eventId}
              AND SoldQuantity + {quantity} <= Capacity
            """, cancellationToken);

        return affectedRows == 1;
    }
}
