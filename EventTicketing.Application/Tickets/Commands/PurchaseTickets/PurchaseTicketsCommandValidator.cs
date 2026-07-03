using FluentValidation;

namespace EventTicketing.Application.Tickets.Commands.PurchaseTickets;

public sealed class PurchaseTicketsCommandValidator : AbstractValidator<PurchaseTicketsCommand>
{
    public PurchaseTicketsCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.PricingTierId).NotEmpty();
        RuleFor(x => x.BuyerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BuyerEmail).NotEmpty().EmailAddress().MaximumLength(300);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10);
    }
}
