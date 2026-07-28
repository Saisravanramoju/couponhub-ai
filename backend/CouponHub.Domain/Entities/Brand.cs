using CouponHub.Domain.Common;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;

namespace CouponHub.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public ImageUrl? LogoUrl { get; private set; }

    public CouponCategory Category { get; private set; }

    public bool IsActive { get; private set; }

    private readonly List<Coupon> _coupons = new();

    public IReadOnlyCollection<Coupon> Coupons => _coupons.AsReadOnly();

    // Required by Entity Framework Core
    private Brand()
    {
    }

    // Constructor to create a new Brand
    public Brand(
        string name,
        CouponCategory category,
        string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Brand name is required.");

        Id = Guid.NewGuid();

        Name = name.Trim();

        Category = category;

        LogoUrl = string.IsNullOrWhiteSpace(logoUrl)? null: ImageUrl.Create(logoUrl);

        IsActive = true;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Brand name is required.");

        Name = name.Trim();
    }

    public void UpdateLogo(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Logo URL is required.");

        LogoUrl = ImageUrl.Create(logoUrl);
    }

    public void UpdateCategory(CouponCategory category)
    {
        Category = category;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
    }
}