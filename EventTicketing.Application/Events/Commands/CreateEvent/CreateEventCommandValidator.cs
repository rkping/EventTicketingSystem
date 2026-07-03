using EventTicketing.Application.Events;
using EventTicketing.Application.Events.commands.CreateEvent;
using FluentValidation;

namespace EventTicketing.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalCapacity).GreaterThan(0);
        RuleFor(x => x.PricingTiers).NotNull().NotEmpty();
        RuleForEach(x => x.PricingTiers).SetValidator(new PricingTierRequestValidator());
        RuleFor(x => x)
            .Must(x => x.PricingTiers is not null && x.PricingTiers.Sum(t => t.Capacity) <= x.TotalCapacity)
            .WithMessage("Sum of pricing tier capacities cannot exceed event total capacity.");
    }
}

public sealed class PricingTierRequestValidator : AbstractValidator<PricingTierRequest>
{
    public PricingTierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
