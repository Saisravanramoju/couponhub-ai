using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using MediatR;

namespace CouponHub.Application.Coupons.Queries.GetCouponById;

public sealed class GetCouponByIdQueryHandler
    : IRequestHandler<GetCouponByIdQuery, Coupon>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public Task<Coupon?> Handle(
        GetCouponByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var coupon = _couponRepository.GetByIdAsync(query.Id, cancellationToken);
        if (coupon is null)
        {
            throw new NotFoundException("Brand", query.Id);
        }
        return coupon;
    }
}
