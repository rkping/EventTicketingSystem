using EventTicketing.Application.Reports;
using EventTicketing.Application.Reports.Commands.GetTicketSalesReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("events/{eventId:guid}/ticket-sales")]
    public async Task<ActionResult<TicketSalesReportResponse>> GetTicketSales(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTicketSalesReportQuery(eventId), cancellationToken);
        return Ok(result);
    }
}
