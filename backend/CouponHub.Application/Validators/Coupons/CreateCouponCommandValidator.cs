using CouponHub.Application.Coupons.Commands.CreateCoupon;
using FluentValidation;

namespace CouponHub.Application.Validators.Coupons;

public sealed class CreateCouponCommandValidator
    : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty()
            .WithMessage("BrandId is required.");

        RuleFor(x => x.CouponCode)
            .NotEmpty()
            .WithMessage("Coupon code is required.")
            .Must(code => code.Trim() == code)
            .WithMessage("Coupon code cannot contain leading or trailing spaces.")
            .MaximumLength(100)
            .WithMessage("Coupon code cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .Must(description => description.Trim() == description)
            .WithMessage("Description cannot contain leading or trailing spaces.")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Invalid coupon category.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("Invalid discount type.");

        RuleFor(x => x.CouponSource)
            .IsInEnum()
            .WithMessage("Invalid coupon source.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum order amount cannot be negative.");

        RuleFor(x => x.MaximumDiscount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaximumDiscount.HasValue)
            .WithMessage("Maximum discount cannot be negative.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future.");

        RuleFor(x => x)
            .Must(x =>
                !x.MaximumDiscount.HasValue ||
                x.MaximumDiscount.Value >= x.DiscountValue)
            .WithMessage("Maximum discount cannot be less than discount value.");
    }
}