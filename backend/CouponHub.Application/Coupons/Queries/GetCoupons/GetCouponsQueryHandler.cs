using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Coupons.Queries.GetCoupons;

public sealed class GetCouponsQueryHandler
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IEnumerable<Coupon>> Handle(
    GetCouponsQuery query,
    CancellationToken cancellationToken = default)
    {
        return await _couponRepository.GetAllAsync(
            cancellationToken);
    }
}
