using EventTicketing.Domain.Entities;

namespace EventTicketing.Tests.Common.Builders;

public sealed class EventBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Tech Conference 2025";
    private string _description = "Annual technology conference";
    private string _venue = "Convention Center";
    private DateOnly _eventDate = new(2025, 6, 15);
    private TimeOnly _eventTime = new(09, 00);
    private int _totalCapacity = 1000;

    public EventBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EventBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EventBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public EventBuilder WithVenue(string venue)
    {
        _venue = venue;
        return this;
    }

    public EventBuilder WithEventDate(DateOnly eventDate)
    {
        _eventDate = eventDate;
        return this;
    }

    public EventBuilder WithEventTime(TimeOnly eventTime)
    {
        _eventTime = eventTime;
        return this;
    }

    public EventBuilder WithTotalCapacity(int totalCapacity)
    {
        _totalCapacity = totalCapacity;
        return this;
    }

    public Event Build()
    {
        return new Event(
            _id,
            _name,
            _description,
            _venue,
            _eventDate,
            _eventTime,
            _totalCapacity);
    }
}
