using EventTicketing.Domain.Entities;

namespace EventTicketing.Tests.Common.Builders;

public sealed class TicketPurchaseBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _eventId = Guid.NewGuid();
    private Guid _pricingTierId = Guid.NewGuid();
    private string _buyerName = "John Doe";
    private string _buyerEmail = "john@example.com";
    private int _quantity = 2;
    private decimal _unitPrice = 150m;
    private DateTimeOffset _purchasedAtUtc = DateTimeOffset.UtcNow;

    public TicketPurchaseBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public TicketPurchaseBuilder WithEventId(Guid eventId)
    {
        _eventId = eventId;
        return this;
    }

    public TicketPurchaseBuilder WithPricingTierId(Guid pricingTierId)
    {
        _pricingTierId = pricingTierId;
        return this;
    }

    public TicketPurchaseBuilder WithBuyerName(string buyerName)
    {
        _buyerName = buyerName;
        return this;
    }

    public TicketPurchaseBuilder WithBuyerEmail(string buyerEmail)
    {
        _buyerEmail = buyerEmail;
        return this;
    }

    public TicketPurchaseBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public TicketPurchaseBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public TicketPurchaseBuilder WithPurchasedAtUtc(DateTimeOffset purchasedAtUtc)
    {
        _purchasedAtUtc = purchasedAtUtc;
        return this;
    }

    public TicketPurchase Build()
    {
        return new TicketPurchase(
            _id,
            _eventId,
            _pricingTierId,
            _buyerName,
            _buyerEmail,
            _quantity,
            _unitPrice,
            _purchasedAtUtc);
    }
}
