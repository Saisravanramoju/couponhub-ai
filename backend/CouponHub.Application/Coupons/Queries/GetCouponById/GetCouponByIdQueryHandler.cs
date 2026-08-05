using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Coupons.Queries.GetCouponById;

public sealed class GetCouponByIdQueryHandler
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public Task<Coupon?> Handle(
        GetCouponByIdQuery query,
        CancellationToken cancellationToken = default) =>
        _couponRepository.GetByIdAsync(query.Id, cancellationToken);
}
