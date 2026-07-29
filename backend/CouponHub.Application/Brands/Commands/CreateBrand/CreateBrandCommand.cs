using CouponHub.Domain.Enums;

namespace CouponHub.Application.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    CouponCategory Category,
    string? LogoUrl);