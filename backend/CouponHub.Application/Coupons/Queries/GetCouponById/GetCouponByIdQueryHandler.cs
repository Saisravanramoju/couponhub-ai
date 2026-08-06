using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;

namespace CouponHub.Application.Coupons.Queries.GetCouponById;

public sealed class GetCouponByIdQueryHandler
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<Coupon> Handle(
     GetCouponByIdQuery query,
     CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (coupon is null)
        {
            throw new NotFoundException(
                "Coupon",
                query.Id);
        }

        return coupon;
    }
}
