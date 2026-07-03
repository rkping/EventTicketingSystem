using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Exceptions;
using MediatR;

namespace TicketingSystem.Application.Events.Commands.DeleteEvent;

public sealed class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteEventCommand request,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetAggregateAsync(
            request.EventId,
            cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException($"Event with id '{request.EventId}' was not found.");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            _eventRepository.Delete(eventEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}