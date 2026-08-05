using CouponHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CouponHub.Api.Contracts.Requests;

public sealed class CreateCouponRequest
{
    [Required]
    public Guid BrandId { get; init; }

    [Required]
    [StringLength(100)]
    public string CouponCode { get; init; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public CouponCategory Category { get; init; }

    [Required]
    public DiscountType DiscountType { get; init; }

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal DiscountValue { get; init; }

    [Range(typeof(decimal), "0", "999999")]
    public decimal? MinimumOrderAmount { get; init; }

    [Range(typeof(decimal), "0", "999999")]
    public decimal? MaximumDiscount { get; init; }

    public DateTime? ExpiryDate { get; init; }

    [Required]
    public CouponSource CouponSource { get; init; }
}