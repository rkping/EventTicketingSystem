using EventTicketing.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventTicketing.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status404NotFound,
                "Not Found",
                ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Unprocessable Entity",
                ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                ex.Message);
        }        
        catch (NotEnoughTicketsException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status406NotAcceptable,
                "Not Acceptable",
                ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);

        await context.Response.WriteAsync(json);
    }
}