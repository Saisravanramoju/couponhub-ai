using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;

namespace CouponHub.Api.Contracts.Responses;

public sealed record BrandResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    BrandCategory Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static BrandResponse FromEntity(Brand brand) => new(
        brand.Id,
        brand.Name,
        brand.LogoUrl?.Value,
        brand.Category,
        brand.IsActive,
        brand.CreatedAt,
        brand.UpdatedAt);
}
