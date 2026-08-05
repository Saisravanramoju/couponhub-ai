using CouponHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CouponHub.Api.Contracts.Requests;

public sealed class CreateBrandRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public BrandCategory Category { get; init; }

    [Url]
    [StringLength(2_048)]
    public string? LogoUrl { get; init; }
}
