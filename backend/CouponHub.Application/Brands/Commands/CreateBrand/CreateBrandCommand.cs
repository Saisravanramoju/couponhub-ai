using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;
using MediatR;

namespace CouponHub.Application.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    BrandCategory Category,
    string? LogoUrl)
    : IRequest<Brand>;