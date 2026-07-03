using EventTicketing.Domain.Entities;

namespace EventTicketing.Application.Events;

public sealed class Event
{
    private readonly List<PricingTier> _pricingTiers = new();

    private Event()
    {
    }

    public Event(
        Guid id,
        string name,
        string description,
        string venue,
        DateOnly eventDate,
        TimeOnly eventTime,
        int totalCapacity)
    {
        Id = id;
        Name = name;
        Description = description;
        Venue = venue;
        EventDate = eventDate;
        EventTime = eventTime;
        TotalCapacity = totalCapacity;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Venue { get; private set; } = default!;

    public DateOnly EventDate { get; private set; }
    public TimeOnly EventTime { get; private set; }

    public int TotalCapacity { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<PricingTier> PricingTiers => _pricingTiers.AsReadOnly();

    public void AddPricingTier(string name, decimal price, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Pricing tier name is required.", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentException("Pricing tier price cannot be negative.", nameof(price));
        }

        if (capacity <= 0)
        {
            throw new ArgumentException("Pricing tier capacity must be greater than zero.", nameof(capacity));
        }

        var currentAllocatedCapacity = _pricingTiers.Sum(tier => tier.Capacity);

        if (currentAllocatedCapacity + capacity > TotalCapacity)
        {
            throw new InvalidOperationException(
                "Pricing tier capacity exceeds total event capacity.");
        }

        _pricingTiers.Add(new PricingTier(
            Guid.NewGuid(),
            Id,
            name.Trim(),
            price,
            capacity));
    }

    public void Update(
    string name,
    string description,
    string venue,
    DateOnly eventDate,
    TimeOnly eventTime,
    int totalCapacity,
    DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Event name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(venue))
        {
            throw new ArgumentException("Venue is required.", nameof(venue));
        }

        if (totalCapacity <= 0)
        {
            throw new ArgumentException("Total capacity must be greater than zero.", nameof(totalCapacity));
        }

        var allocatedTierCapacity = _pricingTiers.Sum(tier => tier.Capacity);

        if (totalCapacity < allocatedTierCapacity)
        {
            throw new InvalidOperationException(
                "Total capacity cannot be less than the total quantity allocated across pricing tiers.");
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Venue = venue.Trim();
        EventDate = eventDate;
        EventTime = eventTime;
        TotalCapacity = totalCapacity;
        UpdatedAtUtc = updatedAtUtc;
        Version++;
    }
}