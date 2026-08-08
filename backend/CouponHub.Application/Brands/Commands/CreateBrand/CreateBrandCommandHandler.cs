using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Brands.Commands.CreateBrand;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using MediatR;


namespace CouponHub.Application.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler
    : IRequestHandler<CreateBrandCommand, Brand>
{
    private readonly IBrandRepository _brandRepository;

    public CreateBrandCommandHandler(
        IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Brand> Handle(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await _brandRepository.ExistsByNameAsync(
            command.Name,
            cancellationToken))
        {
            throw new ConflictException(
                $"Brand '{command.Name}' already exists.");
        }

        var brand = new Brand(
            command.Name,
            command.Category,
            command.LogoUrl);

        await _brandRepository.AddAsync(
            brand,
            cancellationToken);

        return brand;
    }
}