using CouponHub.Application.Abstractions.Specifications;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Specifications.Coupons;

public sealed class CouponByCodeSpecification
    : BaseSpecification<Coupon>
{
    public CouponByCodeSpecification(
        Guid brandId,
        string couponCode)
        : base(c =>
            c.BrandId == brandId &&
            c.CouponCode.ToLower() == couponCode.Trim().ToLower())
    {
    }
}