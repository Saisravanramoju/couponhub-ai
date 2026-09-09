using CouponHub.Application.Abstractions.Specifications;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Specifications.Coupons;

public sealed class CouponByIdSpecification
    : BaseSpecification<Coupon>
{
    public CouponByIdSpecification(Guid id)
        : base(c => c.Id == id)
    {
        AddInclude(c => c.Brand);
    }
}