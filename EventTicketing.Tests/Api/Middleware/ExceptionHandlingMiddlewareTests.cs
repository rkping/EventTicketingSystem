using EventTicketing.Api.Middleware;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace EventTicketing.Tests.Api.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger = new();

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/invalid";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Event not found.";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException(exceptionMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        responseBody.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns422ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Validation failed.";
        var validationException = new ValidationException(exceptionMessage);

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw validationException,
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/123/tickets/purchase";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Not enough tickets available.";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ConflictException(exceptionMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        responseBody.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_NotEnoughTicketsException_Returns406ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/123/tickets/purchase";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Not enough tickets.";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotEnoughTicketsException(exceptionMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status406NotAcceptable);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_DomainException_Returns400ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Invalid domain state.";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DomainException(exceptionMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_Returns500ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Unexpected error"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        responseBody.Should().Contain("An unexpected error occurred");
        responseBody.Should().NotContain("Unexpected error");
    }

    [Fact]
    public async Task InvokeAsync_KnownException_IncludesMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/123";
        context.Response.Body = new MemoryStream();

        var exceptionMessage = "Event with ID 123 not found.";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException(exceptionMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        responseBody.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_HidesInternalDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var secretMessage = "Internal database connection string exposed";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException(secretMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        responseBody.Should().NotContain(secretMessage);
    }
}
