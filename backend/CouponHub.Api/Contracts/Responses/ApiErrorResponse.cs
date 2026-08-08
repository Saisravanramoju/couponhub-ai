namespace CouponHub.Api.Contracts.Responses;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Title,
    IReadOnlyList<string> Errors,
    string TraceId);