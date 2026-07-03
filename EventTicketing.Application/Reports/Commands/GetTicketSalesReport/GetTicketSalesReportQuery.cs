using EventTicketing.Application.Reports;
using MediatR;

namespace EventTicketing.Application.Reports.Commands.GetTicketSalesReport;

public sealed record GetTicketSalesReportQuery(Guid EventId) : IRequest<TicketSalesReportResponse>;
