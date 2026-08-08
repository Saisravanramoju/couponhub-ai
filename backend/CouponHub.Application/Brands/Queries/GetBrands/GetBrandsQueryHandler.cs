using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using MediatR;

namespace CouponHub.Application.Brands.Queries.GetBrands;

public sealed class GetBrandsQueryHandler
    : IRequestHandler<GetBrandsQuery, IEnumerable<Brand>>
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandsQueryHandler(
        IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<IEnumerable<Brand>> Handle(
        GetBrandsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _brandRepository.GetAllAsync(
            cancellationToken);
    }
}