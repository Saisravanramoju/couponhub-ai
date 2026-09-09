using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Specifications.Coupons;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using MediatR;

namespace CouponHub.Application.Coupons.Queries.GetCouponById;

public sealed class GetCouponByIdQueryHandler
    : IRequestHandler<GetCouponByIdQuery, Coupon>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(
        ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<Coupon> Handle(
        GetCouponByIdQuery query,
        CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.FirstOrDefaultAsync(
            new CouponByIdSpecification(query.Id),
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