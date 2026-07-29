using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Common;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Brands.Commands.CreateBrand;

public class CreateBrandCommandHandler
{
    private readonly IBrandRepository _brandRepository;

    public CreateBrandCommandHandler(
        IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Result<Brand>> Handle(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await _brandRepository.ExistsAsync(
            command.Name,
            cancellationToken))
        {
            return Result<Brand>.Failure(
                "Brand already exists.");
        }

        var brand = new Brand(
            command.Name,
            command.Category,
            command.LogoUrl);

        await _brandRepository.AddAsync(
            brand,
            cancellationToken);

        return Result<Brand>.Success(brand);
    }
}