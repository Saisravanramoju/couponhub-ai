using CouponHub.Domain.Entities;
using MediatR;

namespace CouponHub.Application.Brands.Queries.GetBrands;

public sealed record GetBrandsQuery()
    : IRequest<IEnumerable<Brand>>;