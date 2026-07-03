using EventTicketing.Api;
using EventTicketing.Application.Events;
using EventTicketing.Infrastructure.Data;
using EventTicketing.Tests.Common.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EventTicketing.Tests.Api.Controllers;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<TicketingDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TicketingDbContext>(options =>
                options.UseSqlite("DataSource=:memory:"));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}

public sealed class EventsControllerIntegrationTests : IAsyncLifetime
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _httpClient = null!;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _httpClient = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateEvent_InvalidPayload_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateEventRequest(
            "",
            "Description",
            "Venue",
            new DateOnly(2025, 9, 15),
            new TimeOnly(09, 00),
            500,
            new[] { new PricingTierRequest("Standard", 100m, 100) });

        var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }


}
