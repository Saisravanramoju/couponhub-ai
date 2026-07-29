using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Common;
using CouponHub.Domain.Entities;

namespace CouponHub.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler
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

    public async Task<Result<Coupon>> Handle(
        CreateCouponCommand command,
        CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(
            command.BrandId,
            cancellationToken);

        if (brand is null)
        {
            return Result<Coupon>.Failure("Brand does not exist.");
        }

        var coupon = new Coupon(
            brand,
            command.CouponCode,
            command.Description,
            command.Category,
            command.DiscountType,
            command.DiscountValue,
            command.MinimumOrderAmount,
            command.MaximumDiscount,
            command.ExpiryDate,
            command.Source);

        await _couponRepository.AddAsync(
            coupon,
            cancellationToken);

        return Result<Coupon>.Success(coupon);
    }
}