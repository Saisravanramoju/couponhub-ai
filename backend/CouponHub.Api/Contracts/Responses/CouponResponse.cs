using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;

namespace CouponHub.Api.Contracts.Responses;

public sealed record CouponResponse(
    Guid Id,
    Guid BrandId,
    string BrandName,
    string CouponCode,
    string Description,
    CouponCategory Category,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscount,
    DateTime? ExpiryDate,
    CouponSource Source,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static CouponResponse FromEntity(Coupon coupon) => new(
        coupon.Id,
        coupon.BrandId,
        coupon.Brand.Name,
        coupon.CouponCode,
        coupon.Description,
        coupon.Category,
        coupon.DiscountType,
        coupon.DiscountValue,
        coupon.MinimumOrderAmount,
        coupon.MaximumDiscount,
        coupon.ExpiryDate,
        coupon.CouponSource,
        coupon.IsActive,
        coupon.CreatedAt,
        coupon.UpdatedAt);
}
