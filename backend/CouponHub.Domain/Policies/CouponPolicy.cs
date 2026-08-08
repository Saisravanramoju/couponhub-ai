using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;

namespace CouponHub.Domain.Policies;

public static class CouponPolicy
{
    public static void Validate(
      Guid brandId,
      CouponDetails details)
    {
        ValidateCommonRules(
            brandId,
            details.CouponCode,
            details.Description,
            details.DiscountValue,
            details.MinimumOrderAmount,
            details.MaximumDiscount,
            details.ExpiryDate);

        switch (details.DiscountType)
        {
            case DiscountType.Percentage:
                ValidatePercentageDiscount(
                    details.DiscountValue,
                    details.MaximumDiscount);
                break;

            case DiscountType.Flat:
                ValidateFlatDiscount(
                    details.DiscountValue,
                    details.MinimumOrderAmount,
                    details.MaximumDiscount);
                break;

            case DiscountType.Cashback:
                ValidateCashbackDiscount(
                    details.DiscountValue,
                    details.MaximumDiscount);
                break;

            case DiscountType.FreeDelivery:
                ValidateFreeDeliveryDiscount(
                    details.DiscountValue,
                    details.MinimumOrderAmount,
                    details.MaximumDiscount);
                break;

            case DiscountType.BuyOneGetOne:
                ValidateBuyOneGetOneDiscount(
                    details.DiscountValue,
                    details.MinimumOrderAmount,
                    details.MaximumDiscount);
                break;

            case DiscountType.Other:
                break;
        }
    }

    private static void ValidateCommonRules(
     Guid brandId,
     string couponCode,
     string description,
     decimal discountValue,
     decimal? minimumOrderAmount,
     decimal? maximumDiscount,
     DateTime? expiryDate)
    {
        if (brandId == Guid.Empty)
        {
            throw new DomainException("Brand is required.");
        }

        if (string.IsNullOrWhiteSpace(couponCode))
        {
            throw new DomainException("Coupon code is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        if (discountValue < 0)
        {
            throw new DomainException("Discount value cannot be negative.");
        }

        if (minimumOrderAmount.HasValue &&
            minimumOrderAmount.Value < 0)
        {
            throw new DomainException("Minimum order amount cannot be negative.");
        }

        if (maximumDiscount.HasValue &&
            maximumDiscount.Value < 0)
        {
            throw new DomainException("Maximum discount cannot be negative.");
        }

        if (expiryDate.HasValue &&
            expiryDate.Value <= DateTime.UtcNow)
        {
            throw new DomainException(
                "Expiry date must be in the future.");
        }
    }

    private static void ValidatePercentageDiscount(
     decimal discountValue,
     decimal? maximumDiscount)
    {
        if (discountValue <= 0)
        {
            throw new DomainException(
                "Percentage discount must be greater than zero.");
        }

        if (discountValue > 100)
        {
            throw new DomainException(
                "Percentage discount cannot exceed 100%.");
        }

        if (!maximumDiscount.HasValue)
        {
            throw new DomainException(
                "Maximum discount is required for percentage discounts.");
        }

        if (maximumDiscount.Value <= 0)
        {
            throw new DomainException(
                "Maximum discount must be greater than zero.");
        }
    }

    private static void ValidateFlatDiscount(
     decimal discountValue,
     decimal? minimumOrderAmount,
     decimal? maximumDiscount)
    {
        if (discountValue <= 0)
        {
            throw new DomainException(
                "Flat discount must be greater than zero.");
        }

        if (maximumDiscount.HasValue)
        {
            throw new DomainException(
                "Maximum discount is not applicable for flat discounts.");
        }

        if (minimumOrderAmount.HasValue &&
            discountValue >= minimumOrderAmount.Value)
        {
            throw new DomainException(
                "Flat discount must be less than the minimum order amount.");
        }
    }

    private static void ValidateCashbackDiscount(
     decimal discountValue,
     decimal? maximumDiscount)
    {
        if (discountValue <= 0)
        {
            throw new DomainException(
                "Cashback amount must be greater than zero.");
        }

        if (maximumDiscount.HasValue &&
            maximumDiscount.Value <= 0)
        {
            throw new DomainException(
                "Maximum cashback must be greater than zero.");
        }

        if (maximumDiscount.HasValue &&
            discountValue > 100)
        {
            throw new DomainException(
                "Percentage cashback cannot exceed 100%.");
        }
    }

    private static void ValidateFreeDeliveryDiscount(
    decimal discountValue,
    decimal? minimumOrderAmount,
    decimal? maximumDiscount)
    {
        if (discountValue != 0)
        {
            throw new DomainException(
                "Discount value must be zero for free delivery offers.");
        }

        if (maximumDiscount.HasValue)
        {
            throw new DomainException(
                "Maximum discount is not applicable for free delivery offers.");
        }

        if (!minimumOrderAmount.HasValue)
        {
            throw new DomainException(
                "Minimum order amount is required for free delivery offers.");
        }

        if (minimumOrderAmount.Value <= 0)
        {
            throw new DomainException(
                "Minimum order amount must be greater than zero.");
        }
    }

    private static void ValidateBuyOneGetOneDiscount(
    decimal discountValue,
    decimal? minimumOrderAmount,
    decimal? maximumDiscount)
    {
        if (discountValue != 0)
        {
            throw new DomainException(
                "Discount value must be zero for Buy One Get One offers.");
        }

        if (maximumDiscount.HasValue)
        {
            throw new DomainException(
                "Maximum discount is not applicable for Buy One Get One offers.");
        }

        if (!minimumOrderAmount.HasValue)
        {
            throw new DomainException(
                "Minimum order amount is required for Buy One Get One offers.");
        }

        if (minimumOrderAmount.Value <= 0)
        {
            throw new DomainException(
                "Minimum order amount must be greater than zero.");
        }
    }

}