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

    public async Task<Coupon> Handle(
        GetCouponByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _couponRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException("Coupon", query.Id);
    }
}
