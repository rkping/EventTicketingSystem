using EventTicketing.Application.Events.Commands.CreateEvent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Application;

public static class DIServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DIServices).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssemblyContaining<CreateEventCommandValidator>();
        return services;
    }
}
