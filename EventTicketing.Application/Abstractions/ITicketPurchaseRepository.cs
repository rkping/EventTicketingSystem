using EventTicketing.Domain.Entities;

namespace EventTicketing.Application.Abstractions;

public interface ITicketPurchaseRepository
{
    Task AddAsync(TicketPurchase purchase, CancellationToken cancellationToken);
}
