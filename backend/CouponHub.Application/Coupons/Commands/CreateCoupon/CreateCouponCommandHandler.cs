using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;
using MediatR;

namespace CouponHub.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler
    : IRequestHandler<CreateCouponCommand, Coupon>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IBrandRepository _brandRepository;

    public CreateCouponCommandHandler(
        ICouponRepository couponRepository,
        IBrandRepository brandRepository)
    {
        _couponRepository = couponRepository;
        _brandRepository = brandRepository;
    }

    public async Task<Coupon> Handle(
        CreateCouponCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Verify brand exists
        var brand = await _brandRepository.GetByIdAsync(
            command.BrandId,
            cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException(
                "Brand",
                command.BrandId);
        }

        // 2. Verify coupon code is unique for the brand
        if (await _couponRepository.ExistsByCodeAsync(
                command.BrandId,
                command.CouponCode,
                cancellationToken))
        {
            throw new ConflictException(
                $"Coupon code '{command.CouponCode}' already exists for brand '{brand.Name}'.");
        }

        // 3. Let the domain validate itself
        var details = new CouponDetails(
     command.CouponCode,
     command.Description,
     command.Category,
     command.DiscountType,
     command.DiscountValue,
     command.MinimumOrderAmount,
     command.MaximumDiscount,
     command.ExpiryDate,
     command.CouponSource);

        var coupon = new Coupon(
            command.BrandId,
            details);

        // 4. Persist
        coupon = await _couponRepository.AddAsync(
            coupon,
            cancellationToken);

        return coupon;
    }
}