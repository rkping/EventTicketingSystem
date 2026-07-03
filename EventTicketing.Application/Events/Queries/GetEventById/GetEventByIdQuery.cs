using MediatR;

namespace EventTicketing.Application.Events.Queries.GetEventById;

public sealed record GetEventByIdQuery(Guid EventId) : IRequest<EventDetailsResponse>;