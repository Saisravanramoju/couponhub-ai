using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;

namespace CouponHub.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler
{
    private readonly IBrandRepository _brandRepository;
    private readonly ICouponRepository _couponRepository;

    public CreateCouponCommandHandler(
        IBrandRepository brandRepository,
        ICouponRepository couponRepository)
    {
        _brandRepository = brandRepository;
        _couponRepository = couponRepository;
    }

    public async Task<Coupon> Handle(
    CreateCouponCommand command,
    CancellationToken cancellationToken = default)
    {
        // 1. Verify Brand exists
        var brand = await _brandRepository.GetByIdAsync(
            command.BrandId,
            cancellationToken);

        if (brand is null)
        {
            throw new NotFoundException(
                "Brand",
                command.BrandId);
        }

        // 2. Check duplicate coupon code
        if (await _couponRepository.ExistsByCodeAsync(
            command.BrandId,
            command.CouponCode,
            cancellationToken))
        {
            throw new ConflictException(
                $"Coupon code '{command.CouponCode}' already exists for brand '{brand.Name}'.");
        }

        // 3. Create the domain entity
        var coupon = new Coupon(
            command.BrandId,
            command.CouponCode,
            command.Description,
            command.Category,
            command.DiscountType,
            command.DiscountValue,
            command.MinimumOrderAmount,
            command.MaximumDiscount,
            command.ExpiryDate,
            command.CouponSource);

        // 4. Save and return the fully populated entity
        return await _couponRepository.AddAsync(
            coupon,
            cancellationToken);
    }
}