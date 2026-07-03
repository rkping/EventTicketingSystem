using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Application.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDetailsResponse>
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdQueryHandler(IEventRepository eventRepository) => _eventRepository = eventRepository;

    public async Task<EventDetailsResponse> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _eventRepository.GetDetailsAsync(request.EventId, cancellationToken);
        if (result is null)
        {
            throw new NotFoundException("Event not found.");
        }

        return result;
    }
}