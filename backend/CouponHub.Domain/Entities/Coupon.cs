using CouponHub.Domain.Common;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;
using CouponHub.Domain.Policies;

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
        // Required by EF Core
    }

    public Coupon(
    Guid brandId,
    CouponDetails details)
    {
       CouponPolicy.Validate(
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

    

    
    // Method to update the coupon properties
    public void Deactivate()
    {
        if (!IsActive)
            return;
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
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