using EventTicketing.Application;
using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Common;
using EventTicketing.Application.Events;
using EventTicketing.Application.Reports;
using EventTicketing.Domain.Entities;
using EventTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Infrastructure.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly TicketingDbContext _dbContext;

    public EventRepository(TicketingDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken) => await _dbContext.Events.AddAsync(eventEntity, cancellationToken);

    public async Task<Event?> GetAggregateAsync(Guid eventId, CancellationToken cancellationToken) =>
        await _dbContext.Events.Include(x => x.PricingTiers).SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);

    public async Task<EventDetailsResponse?> GetDetailsAsync(Guid eventId, CancellationToken cancellationToken) =>
        await _dbContext.Events.AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailsResponse(
                e.Id,
                e.Name,
                e.Description,
                e.Venue,
                e.EventDate,
                e.EventTime,
                e.TotalCapacity,
                e.PricingTiers.Sum(t => t.SoldQuantity),
                e.TotalCapacity - e.PricingTiers.Sum(t => t.SoldQuantity),
                e.PricingTiers.Select(t => new PricingTierResponse(t.Id, t.Name, t.Price, t.Capacity, t.SoldQuantity, t.Capacity - t.SoldQuantity)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<EventSummaryResponse>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Events.AsNoTracking().OrderBy(x => x.EventDate).ThenBy(x => x.EventTime);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(e => new EventSummaryResponse(
                e.Id,
                e.Name,
                e.Venue,
                e.EventDate,
                e.EventTime,
                e.TotalCapacity,
                e.PricingTiers.Sum(t => t.SoldQuantity),
                e.TotalCapacity - e.PricingTiers.Sum(t => t.SoldQuantity)))
            .ToListAsync(cancellationToken);
        return new PagedResult<EventSummaryResponse>(items, pageNumber, pageSize, total);
    }

    public async Task<AvailabilityResponse?> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken) =>
        await _dbContext.Events.AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new AvailabilityResponse(
                e.Id,
                e.TotalCapacity,
                e.PricingTiers.Sum(t => t.SoldQuantity),
                e.TotalCapacity - e.PricingTiers.Sum(t => t.SoldQuantity),
                e.PricingTiers.Select(t => new TierAvailabilityResponse(t.Id, t.Name, t.Capacity, t.SoldQuantity, t.Capacity - t.SoldQuantity)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<TicketSalesReportResponse?> GetTicketSalesReportAsync(Guid eventId, CancellationToken cancellationToken) =>
        await _dbContext.Events.AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new TicketSalesReportResponse(
                e.Id,
                e.Name,
                e.Venue,
                e.EventDate,
                e.TotalCapacity,
                e.PricingTiers.Sum(t => t.SoldQuantity),
                e.TotalCapacity - e.PricingTiers.Sum(t => t.SoldQuantity),
                _dbContext.TicketPurchases.Where(p => p.EventId == e.Id).Sum(p => (decimal?)p.TotalAmount) ?? 0,
                e.PricingTiers.Select(t => new TierSalesSummaryResponse(
                    t.Id,
                    t.Name,
                    t.Price,
                    t.Capacity,
                    t.SoldQuantity,
                    t.Capacity - t.SoldQuantity,
                    _dbContext.TicketPurchases.Where(p => p.PricingTierId == t.Id).Sum(p => (decimal?)p.TotalAmount) ?? 0)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public void Update(Event eventEntity) => _dbContext.Events.Update(eventEntity);
    public void Delete(Event eventEntity) => _dbContext.Events.Remove(eventEntity);
}
