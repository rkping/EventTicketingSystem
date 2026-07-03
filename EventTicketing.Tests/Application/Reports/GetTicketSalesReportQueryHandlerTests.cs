using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Reports;
using EventTicketing.Application.Reports.Commands.GetTicketSalesReport;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Moq;
using TicketingSystem.Application.Reports.Queries.GetTicketSalesReport;
using Xunit;

namespace EventTicketing.Tests.Application.Reports;

public sealed class GetTicketSalesReportQueryHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly GetTicketSalesReportQueryHandler _handler;

    public GetTicketSalesReportQueryHandlerTests()
    {
        _handler = new GetTicketSalesReportQueryHandler(_mockEventRepository.Object);
    }

    [Fact]
    public async Task Handle_EventExists_ReturnsSalesSummary()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var report = new TicketSalesReportResponse(
            eventId,
            "Tech Conference",
            "Convention Center",
            new DateOnly(2025, 6, 15),
            1000,
            300,
            700,
            45000m,
            new List<TierSalesSummaryResponse>
            {
                new TierSalesSummaryResponse(Guid.NewGuid(), "VIP", 150m, 100, 50, 50, 7500m),
                new TierSalesSummaryResponse(Guid.NewGuid(), "Standard", 75m, 500, 200, 300, 15000m),
                new TierSalesSummaryResponse(Guid.NewGuid(), "Economy", 25m, 400, 50, 350, 1250m)
            });

        _mockEventRepository
            .Setup(x => x.GetTicketSalesReportAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetTicketSalesReportQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(report);
        result.TotalRevenue.Should().Be(45000m);
        result.TotalSoldTickets.Should().Be(300);
    }

    [Fact]
    public async Task Handle_EventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository
            .Setup(x => x.GetTicketSalesReportAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketSalesReportResponse?)null);

        var query = new GetTicketSalesReportQuery(eventId);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_NoPurchases_ReturnsZeroRevenue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var report = new TicketSalesReportResponse(
            eventId,
            "Tech Conference",
            "Convention Center",
            new DateOnly(2025, 6, 15),
            1000,
            0,
            1000,
            0m,
            new List<TierSalesSummaryResponse>
            {
                new TierSalesSummaryResponse(Guid.NewGuid(), "VIP", 150m, 100, 0, 100, 0m),
                new TierSalesSummaryResponse(Guid.NewGuid(), "Standard", 75m, 500, 0, 500, 0m),
                new TierSalesSummaryResponse(Guid.NewGuid(), "Economy", 25m, 400, 0, 400, 0m)
            });

        _mockEventRepository
            .Setup(x => x.GetTicketSalesReportAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetTicketSalesReportQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRevenue.Should().Be(0m);
        result.TotalSoldTickets.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultiplePricingTiers_ReturnsCorrectRevenueByTier()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var vipTierId = Guid.NewGuid();
        var standardTierId = Guid.NewGuid();
        var economyTierId = Guid.NewGuid();

        var report = new TicketSalesReportResponse(
            eventId,
            "Tech Conference",
            "Convention Center",
            new DateOnly(2025, 6, 15),
            1000,
            300,
            700,
            45000m,
            new List<TierSalesSummaryResponse>
            {
                new TierSalesSummaryResponse(vipTierId, "VIP", 150m, 100, 100, 0, 15000m),
                new TierSalesSummaryResponse(standardTierId, "Standard", 75m, 500, 150, 350, 11250m),
                new TierSalesSummaryResponse(economyTierId, "Economy", 25m, 400, 50, 350, 1250m)
            });

        _mockEventRepository
            .Setup(x => x.GetTicketSalesReportAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var query = new GetTicketSalesReportQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.SalesByTier.Should().HaveCount(3);
        result.SalesByTier[0].Revenue.Should().Be(15000m);
        result.SalesByTier[1].Revenue.Should().Be(11250m);
        result.SalesByTier[2].Revenue.Should().Be(1250m);
    }
}
