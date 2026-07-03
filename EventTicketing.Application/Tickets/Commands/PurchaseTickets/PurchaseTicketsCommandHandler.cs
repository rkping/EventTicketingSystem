using EventTicketing.Application;
using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace EventTicketing.Application.Tickets.Commands.PurchaseTickets;

public sealed class PurchaseTicketsCommandHandler : IRequestHandler<PurchaseTicketsCommand, PurchaseTicketResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IPricingTierRepository _pricingTierRepository;
    private readonly ITicketPurchaseRepository _purchaseRepository;
    private readonly ITicketInventoryService _inventoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PurchaseTicketsCommand> _validator;

    public PurchaseTicketsCommandHandler(
        IEventRepository eventRepository,
        IPricingTierRepository pricingTierRepository,
        ITicketPurchaseRepository purchaseRepository,
        ITicketInventoryService inventoryService,
        IUnitOfWork unitOfWork,
        IValidator<PurchaseTicketsCommand> validator)
    {
        _eventRepository = eventRepository;
        _pricingTierRepository = pricingTierRepository;
        _purchaseRepository = purchaseRepository;
        _inventoryService = inventoryService;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<PurchaseTicketResponse> Handle(
    PurchaseTicketsCommand request,
    CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var eventEntity = await _eventRepository.GetAggregateAsync(request.EventId, ct);

            if (eventEntity is null)
            {
                throw new NotFoundException("Event not found.");
            }

            var tier = await _pricingTierRepository.GetAsync(
                request.EventId,
                request.PricingTierId,
                ct);

            if (tier is null)
            {
                throw new NotFoundException("Pricing tier not found.");
            }

            var reserved = await _inventoryService.TryReserveTicketsAsync(
                request.EventId,
                request.PricingTierId,
                request.Quantity,
                ct);

            if (!reserved)
            {
                throw new ConflictException("Not enough tickets available.");
            }

            var purchase = new TicketPurchase(
                Guid.NewGuid(),
                request.EventId,
                request.PricingTierId,
                request.BuyerName,
                request.BuyerEmail,
                request.Quantity,
                tier.Price,
                DateTimeOffset.UtcNow);

            await _purchaseRepository.AddAsync(purchase, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new PurchaseTicketResponse(
                purchase.Id,
                purchase.EventId,
                purchase.PricingTierId,
                purchase.Quantity,
                purchase.UnitPrice,
                purchase.TotalAmount,
                purchase.PurchasedAtUtc);

        }, cancellationToken);
    }
}
