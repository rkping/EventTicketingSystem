using EventTicketing.Application.Common;
using MediatR;

namespace EventTicketing.Application.Events.Queries.ListEvents;

public sealed record ListEventsQuery(int PageNumber, int PageSize) : IRequest<PagedResult<EventSummaryResponse>>;