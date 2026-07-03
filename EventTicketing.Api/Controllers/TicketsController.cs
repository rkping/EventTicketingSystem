using EventTicketing.Application;
using EventTicketing.Application.Tickets.Commands.PurchaseTickets;
using EventTicketing.Application.Tickets.Queries.GetEventAvailability;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public sealed class TicketsController : ControllerBase
{
    private readonly ISender _sender;

    public TicketsController(ISender sender) => _sender = sender;

    [HttpPost("tickets/purchase")]
    public async Task<ActionResult<PurchaseTicketResponse>> Purchase(Guid eventId, PurchaseTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new PurchaseTicketsCommand(
            eventId,
            request.PricingTierId,
            request.BuyerName,
            request.BuyerEmail,
            request.Quantity);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAvailability), new { eventId }, result);
    }

    [HttpGet("availability")]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailability(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEventAvailabilityQuery(eventId), cancellationToken);
        return Ok(result);
    }
}
