using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace EventTicketing.Application.Events.Commands.UpdateEvent;

public sealed class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventDetailsResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateEventCommand> _validator;

    public UpdateEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateEventCommand> validator)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<EventDetailsResponse> Handle(
        UpdateEventCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var eventEntity = await _eventRepository.GetAggregateAsync(
            request.EventId,
            cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException($"Event with id '{request.EventId}' was not found.");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            eventEntity.Update(
            request.Name,
            request.Description,
            request.Venue,
            request.EventDate,
            request.EventTime,
            request.TotalCapacity,
            DateTimeOffset.UtcNow);

            _eventRepository.Update(eventEntity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        var updatedEvent = await _eventRepository.GetDetailsAsync(
            request.EventId,
            cancellationToken);

        if (updatedEvent is null)
        {
            throw new NotFoundException($"Event with id '{request.EventId}' was not found after update.");
        }

        return updatedEvent;
    }
}