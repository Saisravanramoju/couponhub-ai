using CouponHub.Application.Abstractions.Specifications;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Specifications.Brands;

public sealed class BrandByIdSpecification
    : BaseSpecification<Brand>
{
    public BrandByIdSpecification(Guid id)
        : base(b => b.Id == id)
    {
    }
}