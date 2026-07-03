namespace EventTicketing.Application.Reports;

public sealed record TierSalesSummaryResponse(
    Guid PricingTierId,
    string TierName,
    decimal Price,
    int Capacity,
    int SoldQuantity,
    int AvailableQuantity,
    decimal Revenue);

public sealed record TicketSalesReportResponse(
    Guid EventId,
    string EventName,
    string Venue,
    DateOnly EventDate,
    int TotalCapacity,
    int TotalSoldTickets,
    int TotalAvailableTickets,
    decimal TotalRevenue,
    IReadOnlyList<TierSalesSummaryResponse> SalesByTier);
