using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events.commands.CreateEvent;
using FluentValidation;
using MediatR;

namespace EventTicketing.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDetailsResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateEventCommand> _validator;

    public CreateEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateEventCommand> validator)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<EventDetailsResponse> Handle(CreateEventCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        Event? eventEntity = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            eventEntity = new Event(
                Guid.NewGuid(),
                request.Name,
                request.Description,
                request.Venue,
                request.EventDate,
                request.EventTime,
                request.TotalCapacity);

            foreach (var tier in request.PricingTiers)
            {
                eventEntity.AddPricingTier(
                    tier.Name,
                    tier.Price,
                    tier.Capacity);
            }

            await _eventRepository.AddAsync(eventEntity, ct);

            await _unitOfWork.SaveChangesAsync(ct);

        }, cancellationToken);

        var createdEvent = await _eventRepository.GetDetailsAsync(
            eventEntity!.Id,
            cancellationToken);

        if (createdEvent is null)
        {
            throw new InvalidOperationException("Created event could not be loaded.");
        }

        return createdEvent;
    }
}