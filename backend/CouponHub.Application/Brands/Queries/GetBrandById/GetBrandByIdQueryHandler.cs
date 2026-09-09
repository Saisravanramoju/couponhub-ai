using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Specifications.Brands;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using MediatR;


namespace CouponHub.Application.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler
    : IRequestHandler<GetBrandByIdQuery, Brand>
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
        var brand = await _brandRepository.FirstOrDefaultAsync(
                    new BrandByIdSpecification(query.Id),
                    cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException("Brand", query.Id);
        }

        return brand;
    }
}