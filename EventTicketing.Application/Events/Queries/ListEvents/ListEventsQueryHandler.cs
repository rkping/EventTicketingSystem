using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Common;
using MediatR;

namespace EventTicketing.Application.Events.Queries.ListEvents;

public sealed class ListEventsQueryHandler : IRequestHandler<ListEventsQuery, PagedResult<EventSummaryResponse>>
{
    private readonly IEventRepository _eventRepository;

    public ListEventsQueryHandler(IEventRepository eventRepository) => _eventRepository = eventRepository;

    public async Task<PagedResult<EventSummaryResponse>> Handle(ListEventsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var result = await _eventRepository.ListAsync(pageNumber, pageSize, cancellationToken);
        return result;
    }
}
