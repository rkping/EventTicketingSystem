using MediatR;

namespace EventTicketing.Application.Events.Commands.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity) : IRequest<EventDetailsResponse>;
