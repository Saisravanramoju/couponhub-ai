using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using MediatR;

namespace CouponHub.Application.Coupons.Queries.GetCoupons;

public sealed class GetCouponsQueryHandler
    : IRequestHandler<GetCouponsQuery, IEnumerable<Coupon>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public Task<IEnumerable<Coupon>> Handle(
        GetCouponsQuery query,
        CancellationToken cancellationToken = default) =>
        _couponRepository.GetAllAsync(cancellationToken);
}
