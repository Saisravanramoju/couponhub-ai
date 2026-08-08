using CouponHub.Domain.Common;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;

namespace CouponHub.Domain.Entities;

public class Coupon : BaseEntity
{
    public Guid BrandId { get; private set; }
    public Brand Brand { get; private set; } = null!;

    public string CouponCode { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public CouponCategory Category { get; private set; }

    public DiscountType DiscountType { get; private set; }

    public decimal DiscountValue { get; private set; }

    public decimal? MinimumOrderAmount { get; private set; }

    public decimal? MaximumDiscount { get; private set; }

    public DateTime? ExpiryDate { get; private set; }

    public bool IsActive { get; private set; }

    public CouponSource CouponSource { get; private set; }

    private Coupon()
    {
        // Required by Entity Framework Core
    }

    // Constructor to create a new coupon
    public Coupon(
    Guid brandId,
    CouponDetails details)
    {
        // call validation method to validate the coupon properties

        Validate(
       brandId,
       details);

        BrandId = brandId;

        CouponCode = details.CouponCode.Trim();

        Description = details.Description.Trim();

        Category = details.Category;

        DiscountType = details.DiscountType;

        DiscountValue = details.DiscountValue;

        MinimumOrderAmount = details.MinimumOrderAmount;

        MaximumDiscount = details.MaximumDiscount;

        ExpiryDate = details.ExpiryDate;

        CouponSource = details.CouponSource;

        IsActive = true;
    }
    // Validate the coupon properties
    private static void Validate(
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
    // Method to update the coupon properties
    public void Deactivate()
    {
        if (!IsActive)
            return;
        IsActive = false;
        Touch();
    }
    // Method to activate the coupon
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }
    public void UpdateExpiry(DateTime? expiryDate)
    {
        if (expiryDate.HasValue &&
            expiryDate.Value < DateTime.UtcNow)
        {
            throw new DomainException("Expiry date cannot be in the past.");
        }

        ExpiryDate = expiryDate;
        Touch();
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Description = description.Trim();
        Touch();
    }

    public void UpdateDiscount(DiscountType discountType,
    decimal discountValue)
    {
        if (discountValue <= 0)
            throw new DomainException("Discount value must be greater than zero.");

        DiscountType = discountType;
        DiscountValue = discountValue;

        Touch();
    }

    public void UpdateCategory(CouponCategory category)
    {
        Category = category;
        Touch();
    }

}
