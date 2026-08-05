namespace CouponHub.Api.Contracts.Responses;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Title,
    string Detail,
    string TraceId);
