using CouponHub.Api.Contracts.Responses;
using CouponHub.Domain.Exceptions;
using System.Text.Json;

namespace CouponHub.Api.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
     HttpContext context,
     Exception exception)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred. TraceId: {TraceId}",
            context.TraceIdentifier);

        var (statusCode, title, detail) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                ex.Message),

            ConflictException ex => (
                StatusCodes.Status409Conflict,
                "Conflict",
                ex.Message),

            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                ex.Message),

            DomainException ex => (
                StatusCodes.Status400BadRequest,
                "Domain Error",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiErrorResponse(
            statusCode,
            title,
            detail,
            context.TraceIdentifier);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}