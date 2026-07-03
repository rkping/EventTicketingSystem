using EventTicketing.Application.Events;
using EventTicketing.Application.Events.commands.CreateEvent;

namespace EventTicketing.Tests.Common.Builders;

public sealed class CreateEventCommandBuilder
{
    private string _name = "Tech Conference 2025";
    private string _description = "Annual technology conference";
    private string _venue = "Convention Center";
    private DateOnly _eventDate = new(2025, 6, 15);
    private TimeOnly _eventTime = new(09, 00);
    private int _totalCapacity = 1000;
    private List<PricingTierRequest> _pricingTiers = new()
    {
        new PricingTierRequest("VIP", 150m, 100),
        new PricingTierRequest("Standard", 75m, 500),
        new PricingTierRequest("Economy", 25m, 400)
    };

    public CreateEventCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateEventCommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateEventCommandBuilder WithVenue(string venue)
    {
        _venue = venue;
        return this;
    }

    public CreateEventCommandBuilder WithEventDate(DateOnly eventDate)
    {
        _eventDate = eventDate;
        return this;
    }

    public CreateEventCommandBuilder WithEventTime(TimeOnly eventTime)
    {
        _eventTime = eventTime;
        return this;
    }

    public CreateEventCommandBuilder WithTotalCapacity(int totalCapacity)
    {
        _totalCapacity = totalCapacity;
        return this;
    }

    public CreateEventCommandBuilder WithPricingTiers(params PricingTierRequest[] tiers)
    {
        _pricingTiers = tiers.ToList();
        return this;
    }

    public CreateEventCommand Build()
    {
        return new CreateEventCommand(
            _name,
            _description,
            _venue,
            _eventDate,
            _eventTime,
            _totalCapacity,
            _pricingTiers);
    }
}
