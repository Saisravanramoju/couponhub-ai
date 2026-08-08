using CouponHub.Domain.Entities;
using MediatR;

public sealed record GetBrandByIdQuery(Guid Id)
    : IRequest<Brand>;