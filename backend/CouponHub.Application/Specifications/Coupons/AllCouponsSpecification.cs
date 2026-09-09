using CouponHub.Application.Abstractions.Specifications;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Specifications.Coupons;

public sealed class AllCouponsSpecification
    : BaseSpecification<Coupon>
{
    public AllCouponsSpecification()
    {
        AddInclude(c => c.Brand);

        ApplyOrderBy(c => c.Brand.Name);
    }
}