using EventTicketing.Application;
using MediatR;

namespace EventTicketing.Application.Tickets.Queries.GetEventAvailability;

public sealed record GetEventAvailabilityQuery(Guid EventId) : IRequest<AvailabilityResponse>;
