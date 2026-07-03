using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Entities;
using EventTicketing.Infrastructure.Data;

namespace EventTicketing.Infrastructure.Repositories;

public sealed class TicketPurchaseRepository : ITicketPurchaseRepository
{
    private readonly TicketingDbContext _dbContext;
    public TicketPurchaseRepository(TicketingDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(TicketPurchase purchase, CancellationToken cancellationToken) => await _dbContext.TicketPurchases.AddAsync(purchase, cancellationToken);
}
