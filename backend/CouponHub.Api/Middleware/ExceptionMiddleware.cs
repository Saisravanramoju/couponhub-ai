using CouponHub.Api.Contracts.Responses;
using FluentValidation;
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
            "Unhandled exception. TraceId: {TraceId}",
            context.TraceIdentifier);

        var(statusCode, title, errors) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                ex.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList()),

            ConflictException ex => (
                StatusCodes.Status409Conflict,
                "Conflict",
                new List<string> { ex.Message }),

            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                new List<string> { ex.Message }),

            DomainException ex => (
                StatusCodes.Status400BadRequest,
                "Domain Error",
                new List<string> { ex.Message }),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                new List<string>
                {
            "An unexpected error occurred."
                })
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            statusCode,
            title,
            errors,
            context.TraceIdentifier);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}