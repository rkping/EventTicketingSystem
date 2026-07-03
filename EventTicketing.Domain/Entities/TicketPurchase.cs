using EventTicketing.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Domain.Entities;

public sealed class TicketPurchase
{
    private TicketPurchase() { }

    public TicketPurchase(Guid id, Guid eventId, Guid pricingTierId, string buyerName, string buyerEmail, int quantity, decimal unitPrice, DateTimeOffset purchasedAtUtc)
    {
        if (id == Guid.Empty) throw new DomainException("Purchase id is required.");
        if (eventId == Guid.Empty) throw new DomainException("Event id is required.");
        if (pricingTierId == Guid.Empty) throw new DomainException("Pricing tier id is required.");
        if (string.IsNullOrWhiteSpace(buyerName)) throw new DomainException("Buyer name is required.");
        if (string.IsNullOrWhiteSpace(buyerEmail)) throw new DomainException("Buyer email is required.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");

        Id = id;
        EventId = eventId;
        PricingTierId = pricingTierId;
        BuyerName = buyerName.Trim();
        BuyerEmail = buyerEmail.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = quantity * unitPrice;
        PurchasedAtUtc = purchasedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Event Event { get; private set; } = null!;
    public Guid PricingTierId { get; private set; }
    public PricingTier PricingTier { get; private set; } = null!;
    public string BuyerName { get; private set; } = string.Empty;
    public string BuyerEmail { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset PurchasedAtUtc { get; private set; }
}
