using EventTicketing.Application;
using MediatR;

namespace EventTicketing.Application.Tickets.Commands.PurchaseTickets;

public sealed record PurchaseTicketsCommand(
    Guid EventId,
    Guid PricingTierId,
    string BuyerName,
    string BuyerEmail,
    int Quantity) : IRequest<PurchaseTicketResponse>;
