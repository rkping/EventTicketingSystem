using FluentValidation;

namespace EventTicketing.Application.Events.Commands.UpdateEvent;

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalCapacity).GreaterThan(0);
    }
}
