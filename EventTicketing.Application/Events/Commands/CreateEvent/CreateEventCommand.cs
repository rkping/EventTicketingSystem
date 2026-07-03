using MediatR;

namespace EventTicketing.Application.Events.commands.CreateEvent;

public sealed record CreateEventCommand(
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity,
    IEnumerable<PricingTierRequest> PricingTiers) : IRequest<EventDetailsResponse>;