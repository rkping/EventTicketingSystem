using MediatR;

namespace TicketingSystem.Application.Events.Commands.DeleteEvent;

public sealed record DeleteEventCommand(Guid EventId) : IRequest;
