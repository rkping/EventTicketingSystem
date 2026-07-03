using EventTicketing.Application.Abstractions;
using EventTicketing.Infrastructure.Data;
using EventTicketing.Infrastructure.Repositories;
using EventTicketing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Infrastructure;

public static class DIServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketingDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TicketingDbContext>());
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IPricingTierRepository, PricingTierRepository>();
        services.AddScoped<ITicketPurchaseRepository, TicketPurchaseRepository>();
        services.AddScoped<ITicketInventoryService, TicketInventoryService>();
        return services;

    }
}
