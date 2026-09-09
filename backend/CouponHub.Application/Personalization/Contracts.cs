using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;

namespace CouponHub.Application.Personalization;

public interface ICurrentUser
{
    Guid? Id { get; }
    bool IsAdmin { get; }
}

public sealed record RecommendationRequest(string Query = "", decimal? OrderAmount = null,
    CouponCategory? Category = null, int Limit = 10, bool UseAi = false);
public sealed record RankedCoupon(Coupon Coupon, decimal? EstimatedSavings, string Reason);
public sealed record RecommendationResult(string Mode, IReadOnlyList<RankedCoupon> Items);
public sealed record AiCandidate(Guid Id, string Brand, string Description, string Category,
    decimal? EstimatedSavings, DateTime? ExpiryDate);
public interface ICouponAi
{
    bool IsConfigured { get; }
    Task<IReadOnlyList<Guid>> RankAsync(string query, IReadOnlyList<AiCandidate> candidates, CancellationToken ct);
    Task<ExtractedCoupon> ExtractAsync(string text, CancellationToken ct);
}
public sealed record ExtractedCoupon(string? BrandName, string? CouponCode, string? Description,
    CouponCategory? Category, DiscountType? DiscountType, decimal? DiscountValue,
    decimal? MinimumOrderAmount, decimal? MaximumDiscount, DateTime? ExpiryDate);

public static class CouponRanking
{
    public static bool Eligible(Coupon c, DateTime now, decimal? orderAmount) =>
        c.IsActive && c.Brand.IsActive && (!c.ExpiryDate.HasValue || c.ExpiryDate > now) &&
        (!orderAmount.HasValue || !c.MinimumOrderAmount.HasValue || orderAmount >= c.MinimumOrderAmount);

    // Cashback is an estimate, not an immediate checkout reduction.
    // Free delivery / BOGO require basket details; never invent monetary savings.
    public static decimal? Savings(Coupon c, decimal? orderAmount)
    {
        if (!orderAmount.HasValue) return null;
        var amount = orderAmount.Value;
        decimal? saving = c.DiscountType switch
        {
            DiscountType.Flat => c.DiscountValue,
            DiscountType.Percentage => amount * c.DiscountValue / 100m,
            DiscountType.Cashback => c.MaximumDiscount.HasValue ? amount * c.DiscountValue / 100m : c.DiscountValue,
            _ => null
        };
        if (!saving.HasValue) return null;
        return decimal.Round(Math.Min(amount, Math.Min(saving.Value, c.MaximumDiscount ?? decimal.MaxValue)), 2);
    }

    public static double Score(Coupon c, string query, IReadOnlyCollection<string> preferences,
        IReadOnlySet<Guid> favorites, decimal? amount)
    {
        var text = $"{c.Brand.Name} {c.Category} {c.Description}";
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase);
        return words.Count(w => text.Contains(w, StringComparison.OrdinalIgnoreCase)) * 20
            + (preferences.Contains(c.Category.ToString()) ? 12 : 0)
            + (favorites.Contains(c.Id) ? 8 : 0)
            + (double)(Savings(c, amount) ?? 0) / Math.Max(1, (double)(amount ?? 100));
    }
}
