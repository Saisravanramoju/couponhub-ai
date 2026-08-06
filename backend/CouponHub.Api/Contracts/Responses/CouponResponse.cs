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
    bool IsActive,
    CouponSource CouponSource,
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
        coupon.IsActive,
        coupon.CouponSource,
        coupon.CreatedAt,
        coupon.UpdatedAt);
}