namespace EventTicketing.Application.Events;

public sealed record CreateEventRequest(
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity,
    IEnumerable<PricingTierRequest> PricingTiers);

public sealed record PricingTierRequest(
    string Name,
    decimal Price,
    int Capacity);

public sealed record UpdateEventRequest(
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity);

public sealed record PricingTierResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Capacity,
    int SoldQuantity,
    int AvailableQuantity);

public sealed record EventDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity,
    int SoldQuantity,
    int AvailableQuantity,
    IReadOnlyList<PricingTierResponse> PricingTiers);

public sealed record EventSummaryResponse(
    Guid Id,
    string Name,
    string Venue,
    DateOnly EventDate,
    TimeOnly EventTime,
    int TotalCapacity,
    int SoldQuantity,
    int AvailableQuantity);