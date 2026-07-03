using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Reports;
using EventTicketing.Application.Reports.Commands.GetTicketSalesReport;
using EventTicketing.Domain.Exceptions;
using MediatR;

namespace TicketingSystem.Application.Reports.Queries.GetTicketSalesReport;

public sealed class GetTicketSalesReportQueryHandler : IRequestHandler<GetTicketSalesReportQuery, TicketSalesReportResponse>
{
    private readonly IEventRepository _eventRepository;

    public GetTicketSalesReportQueryHandler(IEventRepository eventRepository) => _eventRepository = eventRepository;

    public async Task<TicketSalesReportResponse> Handle(GetTicketSalesReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _eventRepository.GetTicketSalesReportAsync(request.EventId, cancellationToken);
        if (report == null)
        {
            throw new NotFoundException($"Event with ID {request.EventId} not found.");
        }
        return report;
    }
}
