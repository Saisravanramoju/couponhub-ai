using CouponHub.Domain.Enums;
using CouponHub.Domain.Entities;
using MediatR;
namespace CouponHub.Application.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(
    Guid BrandId,
    string CouponCode,
    string Description,
    CouponCategory Category,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscount,
    DateTime? ExpiryDate,
    CouponSource CouponSource): IRequest<Coupon>;
