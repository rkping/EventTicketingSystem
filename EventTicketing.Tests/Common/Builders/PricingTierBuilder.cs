using EventTicketing.Domain.Entities;

namespace EventTicketing.Tests.Common.Builders;

public sealed class PricingTierBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _eventId = Guid.NewGuid();
    private string _name = "VIP";
    private decimal _price = 150m;
    private int _capacity = 100;

    public PricingTierBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PricingTierBuilder WithEventId(Guid eventId)
    {
        _eventId = eventId;
        return this;
    }

    public PricingTierBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PricingTierBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public PricingTierBuilder WithCapacity(int capacity)
    {
        _capacity = capacity;
        return this;
    }

    public PricingTier Build()
    {
        return new PricingTier(_id, _eventId, _name, _price, _capacity);
    }
}
