
namespace EventTicketing.Application;

public sealed record PurchaseTicketRequest(
    Guid PricingTierId,
    string BuyerName,
    string BuyerEmail,
    int Quantity);

public sealed record PurchaseTicketResponse(
    Guid PurchaseId,
    Guid EventId,
    Guid PricingTierId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTimeOffset PurchasedAtUtc);

public sealed record TierAvailabilityResponse(
    Guid PricingTierId,
    string Name,
    int Capacity,
    int SoldQuantity,
    int AvailableQuantity);

public sealed record AvailabilityResponse(
    Guid EventId,
    int TotalCapacity,
    int SoldTickets,
    int AvailableTickets,
    IReadOnlyList<TierAvailabilityResponse> PricingTiers);
