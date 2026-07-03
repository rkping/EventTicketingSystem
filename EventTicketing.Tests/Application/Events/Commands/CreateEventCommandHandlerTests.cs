using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events;
using EventTicketing.Application.Events.commands.CreateEvent;
using EventTicketing.Application.Events.Commands.CreateEvent;
using EventTicketing.Domain.Entities;
using EventTicketing.Tests.Common.Builders;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace EventTicketing.Tests.Application.Events.Commands;

public sealed class CreateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly CreateEventCommandValidator _validator = new();
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _handler = new CreateEventCommandHandler(
            _mockEventRepository.Object,
            _mockUnitOfWork.Object,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsEventAndSavesChanges()
    {
        // Arrange
        var command = new CreateEventCommandBuilder().Build();
        var eventId = Guid.NewGuid();
        var createdEvent = new EventDetailsResponse(
            eventId,
            command.Name,
            command.Description,
            command.Venue,
            command.EventDate,
            command.EventTime,
            command.TotalCapacity,
            0,
            command.TotalCapacity,
            command.PricingTiers
                .Select(t => new PricingTierResponse(Guid.NewGuid(), t.Name, t.Price, t.Capacity, 0, t.Capacity))
                .ToList());

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task> action, CancellationToken ct) => await action(ct))
            .Returns(Task.CompletedTask);

        _mockEventRepository
            .Setup(x => x.GetDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        _mockEventRepository.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(
            x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsEventDetailsResponse()
    {
        // Arrange
        var command = new CreateEventCommandBuilder().Build();
        var expectedResponse = new EventDetailsResponse(
            Guid.NewGuid(),
            command.Name,
            command.Description,
            command.Venue,
            command.EventDate,
            command.EventTime,
            command.TotalCapacity,
            0,
            command.TotalCapacity,
            command.PricingTiers
                .Select(t => new PricingTierResponse(Guid.NewGuid(), t.Name, t.Price, t.Capacity, 0, t.Capacity))
                .ToList());

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task> action, CancellationToken ct) => await action(ct))
            .Returns(Task.CompletedTask);

        _mockEventRepository
            .Setup(x => x.GetDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateEventCommandBuilder()
            .WithName("")
            .Build();

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
        _mockEventRepository.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNullAfterCreate_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreateEventCommandBuilder().Build();

        _mockUnitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Callback(async (Func<CancellationToken, Task> action, CancellationToken ct) => await action(ct))
            .Returns(Task.CompletedTask);

        _mockEventRepository
            .Setup(x => x.GetDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDetailsResponse?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Created event could not be loaded.");
    }
}
