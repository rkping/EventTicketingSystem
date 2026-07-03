using EventTicketing.Application.Tickets.Commands.PurchaseTickets;

namespace EventTicketing.Tests.Common.Builders;

public sealed class PurchaseTicketsCommandBuilder
{
    private Guid _eventId = Guid.NewGuid();
    private Guid _pricingTierId = Guid.NewGuid();
    private string _buyerName = "John Doe";
    private string _buyerEmail = "john@example.com";
    private int _quantity = 2;

    public PurchaseTicketsCommandBuilder WithEventId(Guid eventId)
    {
        _eventId = eventId;
        return this;
    }

    public PurchaseTicketsCommandBuilder WithPricingTierId(Guid pricingTierId)
    {
        _pricingTierId = pricingTierId;
        return this;
    }

    public PurchaseTicketsCommandBuilder WithBuyerName(string buyerName)
    {
        _buyerName = buyerName;
        return this;
    }

    public PurchaseTicketsCommandBuilder WithBuyerEmail(string buyerEmail)
    {
        _buyerEmail = buyerEmail;
        return this;
    }

    public PurchaseTicketsCommandBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public PurchaseTicketsCommand Build()
    {
        return new PurchaseTicketsCommand(
            _eventId,
            _pricingTierId,
            _buyerName,
            _buyerEmail,
            _quantity);
    }
}
