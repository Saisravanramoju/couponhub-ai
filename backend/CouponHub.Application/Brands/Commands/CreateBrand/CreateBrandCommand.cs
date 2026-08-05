using CouponHub.Domain.Enums;

namespace CouponHub.Application.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    BrandCategory Category,
    string? LogoUrl);