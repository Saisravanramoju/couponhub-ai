namespace CouponHub.Domain.ValueObjects;

using CouponHub.Domain.Enums;

public sealed record CouponDetails(
    string CouponCode,
    string Description,
    CouponCategory Category,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscount,
    DateTime? ExpiryDate,
    CouponSource CouponSource);