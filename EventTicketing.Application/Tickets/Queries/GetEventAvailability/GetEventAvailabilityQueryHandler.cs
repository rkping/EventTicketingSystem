using EventTicketing.Application;
using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Tickets.Queries.GetEventAvailability;
using EventTicketing.Domain.Exceptions;
using MediatR;

namespace TicketingSystem.Application.Tickets.Queries.GetEventAvailability;

public sealed class GetEventAvailabilityQueryHandler : IRequestHandler<GetEventAvailabilityQuery, AvailabilityResponse>
{
    private readonly IEventRepository _eventRepository;

    public GetEventAvailabilityQueryHandler(IEventRepository eventRepository) => _eventRepository = eventRepository;

    public async Task<AvailabilityResponse> Handle(GetEventAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var availability = await _eventRepository.GetAvailabilityAsync(request.EventId, cancellationToken);
        if (availability == null)
            throw new NotFoundException($"Event with ID {request.EventId} not found.");
        else
            return availability;
    }
}
