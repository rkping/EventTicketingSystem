using EventTicketing.Api.Middleware;
using EventTicketing.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace EventTicketing.Tests.Api.Middleware;

/// <summary>
/// Comprehensive tests for exception handling middleware ProblemDetails responses
/// </summary>
public sealed class ExceptionHandlingMiddlewareProblemDetailsTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger = new();

    [Fact]
    public async Task InvokeAsync_NotFoundException_ReturnsProblemDetailsWithCorrectStructure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/123";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Event with ID 123 not found"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("detail").GetString().Should().Be("Event with ID 123 not found");
        problemDetails.GetProperty("instance").GetString().Should().Be("/api/events/123");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsProblemDetailsWithCorrectStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new FluentValidation.ValidationException("Validation failed"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(422);
        problemDetails.GetProperty("title").GetString().Should().Be("Unprocessable Entity");
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_ReturnsProblemDetailsWithCorrectStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events/123/tickets";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ConflictException("Duplicate event name"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(409);
        problemDetails.GetProperty("title").GetString().Should().Be("Conflict");
    }

    [Fact]
    public async Task InvokeAsync_NotEnoughTicketsException_ReturnsProblemDetailsWithCorrectStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tickets/purchase";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotEnoughTicketsException("Only 5 tickets remaining"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status406NotAcceptable);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(406);
        problemDetails.GetProperty("title").GetString().Should().Be("Not Acceptable");
        problemDetails.GetProperty("detail").GetString().Should().Be("Only 5 tickets remaining");
    }

    [Fact]
    public async Task InvokeAsync_DomainException_ReturnsProblemDetailsWithCorrectStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DomainException("Invalid business rule"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("title").GetString().Should().Be("Bad Request");
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_RedactsDetailMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var sensitiveInfo = "Server=production.database.com;Password=SecretPassword123";
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException(sensitiveInfo),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Should contain generic message, not the sensitive details
        problemDetails.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
        responseBody.Should().NotContain(sensitiveInfo);
        responseBody.Should().NotContain("SecretPassword");
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_LogsTheException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var exceptionToThrow = new InvalidOperationException("Test exception");
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exceptionToThrow,
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e == exceptionToThrow),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_KnownExceptions_DoNotLog()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Event not found"),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Error should not be logged for known exceptions
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_EmptyExceptionMessage_IncludesGenericDetail()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException(""),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Should have empty or generic detail
        problemDetails.GetProperty("detail").GetString().Should().Be("");
    }

    [Fact]
    public async Task InvokeAsync_LongExceptionMessage_IncludesFullMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/events";
        context.Response.Body = new MemoryStream();

        var longMessage = string.Concat(Enumerable.Repeat("This is a very long error message. ", 50));
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException(longMessage),
            _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Should include the full message
        problemDetails.GetProperty("detail").GetString().Should().Be(longMessage);
    }

    [Fact]
    public async Task InvokeAsync_MultipleExceptionTypes_AllHandledCorrectly()
    {
        // Test various exceptions in sequence
        var exceptions = new List<(Exception ex, int expectedStatus, string expectedTitle)>
        {
            (new NotFoundException("Not found"), 404, "Not Found"),
            (new ConflictException("Conflict"), 409, "Conflict"),
            (new DomainException("Domain error"), 400, "Bad Request"),
            (new NotEnoughTicketsException("No tickets"), 406, "Not Acceptable"),
            (new FluentValidation.ValidationException("Validation failed"), 422, "Unprocessable Entity"),
        };

        foreach (var (ex, expectedStatus, expectedTitle) in exceptions)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test";
            context.Response.Body = new MemoryStream();

            var middleware = new ExceptionHandlingMiddleware(
                _ => throw ex,
                _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(expectedStatus, $"Failed for {ex.GetType().Name}");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);
            problemDetails.GetProperty("title").GetString().Should().Be(expectedTitle);
        }
    }
}
