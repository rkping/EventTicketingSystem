using EventTicketing.Application.Common;
using EventTicketing.Application.Events;
using EventTicketing.Application.Reports;
using EventTicketing.Domain.Entities;

namespace EventTicketing.Application.Abstractions;

public interface IEventRepository
{
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken);
    Task<Event?> GetAggregateAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EventDetailsResponse?> GetDetailsAsync(Guid eventId, CancellationToken cancellationToken);
    Task<PagedResult<EventSummaryResponse>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<AvailabilityResponse?> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken);
    Task<TicketSalesReportResponse?> GetTicketSalesReportAsync(Guid eventId, CancellationToken cancellationToken);
    void Update(Event eventEntity);
    void Delete(Event eventEntity);
}
