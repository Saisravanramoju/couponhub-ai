using CouponHub.Domain.Entities;
using MediatR;

public sealed record GetCouponsQuery()
    : IRequest<IEnumerable<Coupon>>;