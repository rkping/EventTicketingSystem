using EventTicketing.Application.Events;
using EventTicketing.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Domain.Entities;

public sealed class PricingTier
{
    private PricingTier() { }

    public PricingTier(Guid id, Guid eventId, string name, decimal price, int capacity)
    {
        if (id == Guid.Empty) throw new DomainException("Pricing tier id is required.");
        if (eventId == Guid.Empty) throw new DomainException("Event id is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Pricing tier name is required.");
        if (price < 0) throw new DomainException("Price cannot be negative.");
        if (capacity <= 0) throw new DomainException("Pricing tier capacity must be greater than zero.");

        Id = id;
        EventId = eventId;
        Name = name.Trim();
        Price = price;
        Capacity = capacity;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Event Event { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Capacity { get; private set; }
    public int SoldQuantity { get; private set; }
    public int Version { get; private set; }
    public int AvailableQuantity => Capacity - SoldQuantity;

    public void EnsureCanPurchase(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (SoldQuantity + quantity > Capacity) throw new NotEnoughTicketsException("Not enough tickets available.");
    }

    public void MarkSoldForDomainTests(int quantity)
    {
        EnsureCanPurchase(quantity);
        SoldQuantity += quantity;
        Version++;
    }
}