using CouponHub.Domain.Common;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;

namespace CouponHub.Domain.Entities;

public class Coupon : BaseEntity
{
    public Guid BrandId { get; private set; }

    // Navigation property populated by EF Core when queried with Include()
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
        // Required by EF Core
    }

    public Coupon(
        Guid brandId,
        string couponCode,
        string description,
        CouponCategory category,
        DiscountType discountType,
        decimal discountValue,
        decimal? minimumOrderAmount,
        decimal? maximumDiscount,
        DateTime? expiryDate,
        CouponSource couponSource)
    {
        Validate(
            couponCode,
            description,
            discountValue,
            minimumOrderAmount,
            maximumDiscount,
            expiryDate);

        BrandId = brandId;

        CouponCode = couponCode.Trim();

        Description = description.Trim();

        Category = category;

        DiscountType = discountType;

        DiscountValue = discountValue;

        MinimumOrderAmount = minimumOrderAmount;

        MaximumDiscount = maximumDiscount;

        ExpiryDate = expiryDate;

        CouponSource = couponSource;

        IsActive = true;
    }

    private static void Validate(
        string couponCode,
        string description,
        decimal discountValue,
        decimal? minimumOrderAmount,
        decimal? maximumDiscount,
        DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            throw new DomainException("Coupon code is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        if (discountValue <= 0)
            throw new DomainException("Discount value must be greater than zero.");

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
            expiryDate.Value < DateTime.UtcNow)
        {
            throw new DomainException("Expiry date cannot be in the past.");
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Description = description.Trim();

        Touch();
    }

    public void UpdateDiscount(
        DiscountType discountType,
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
}