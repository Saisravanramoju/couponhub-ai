using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Common;
using CouponHub.Domain.Exceptions;

namespace CouponHub.Domain.Entities;

public class Coupon : BaseEntity
{
    public Brand Brand { get; private set; }

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
        Brand brand,
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
        // call validation method to validate the coupon properties

        Validate(
    brand,
    couponCode,
    description,
    discountValue,
    minimumOrderAmount,
    maximumDiscount,
    expiryDate);
       
        Brand = brand;

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
    // Validate the coupon properties
    private static void Validate(
    Brand brand,
    string couponCode,
    string description,
    decimal discountValue,
    decimal? minimumOrderAmount,
    decimal? maximumDiscount,
    DateTime? expiryDate)
    {
        if (brand is null)
            throw new DomainException("Brand is required.");

        if (string.IsNullOrWhiteSpace(couponCode))
            throw new DomainException("Coupon code is required.");

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

        if (expiryDate.HasValue && expiryDate.Value < DateTime.UtcNow)
            throw new DomainException("Expiry date cannot be in the past.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
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
    public void UpdateExpiry(DateTime expiryDate)
    {
        if (expiryDate < DateTime.UtcNow)
            throw new DomainException("Expiry date cannot be in the past.");

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
