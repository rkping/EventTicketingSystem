namespace EventTicketing.Application.Abstractions;

public interface ITicketInventoryService
{
    Task<bool> TryReserveTicketsAsync(Guid eventId, Guid pricingTierId, int quantity, CancellationToken cancellationToken);
}
