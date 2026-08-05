using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;

namespace CouponHub.Application.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandByIdQueryHandler(
        IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Brand> Handle(
        GetBrandByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException("Brand", query.Id);
        }

        return brand;
    }
}