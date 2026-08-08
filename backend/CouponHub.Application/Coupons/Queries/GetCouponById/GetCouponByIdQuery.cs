using CouponHub.Domain.Entities;
using MediatR;

public sealed record GetCouponByIdQuery(Guid Id)
    : IRequest<Coupon>;