using CouponHub.Application.Personalization;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Exceptions;
using CouponHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CouponHub.Infrastructure.Personalization;

public sealed class RecommendationService(ApplicationDbContext db, ICurrentUser user, ICouponAi ai,
    ILogger<RecommendationService> logger)
{
    public async Task<RecommendationResult> Recommend(RecommendationRequest request, CancellationToken ct)
    {
        if (request.Limit is < 1 or > 50 || request.Query.Length > 500 || request.OrderAmount is < 0 or > 999999)
            throw new DomainException("Limit must be 1–50, query at most 500 characters, and order amount 0–999999 INR.");
        if (request.Category.HasValue && !Enum.IsDefined(request.Category.Value)) throw new DomainException("Invalid category.");
        var now = DateTime.UtcNow;
        var query = db.Coupons.AsNoTracking().Include(c => c.Brand)
            .Where(c => c.IsActive && c.Brand.IsActive && (c.ExpiryDate == null || c.ExpiryDate > now));
        if (request.Category.HasValue) query = query.Where(c => c.Category == request.Category);
        if (request.OrderAmount.HasValue) query = query.Where(c => c.MinimumOrderAmount == null || c.MinimumOrderAmount <= request.OrderAmount);
        // Bounded retrieval: rank up to 200 recent eligible coupons; this is not a full vector index.
        var candidates = await query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id).Take(200).ToListAsync(ct);
        var preferences = await db.Set<Account>().Where(a => a.Id == user.Id).Select(a => a.Categories).SingleAsync(ct);
        var saved = (await db.Set<SavedCoupon>().Where(s => s.AccountId == user.Id).Select(s => s.CouponId).ToListAsync(ct)).ToHashSet();
        var ranked = candidates.OrderByDescending(c => CouponRanking.Score(c, request.Query, preferences, saved, request.OrderAmount))
            .ThenBy(c => c.ExpiryDate ?? DateTime.MaxValue).ThenBy(c => c.Id).ToList();
        var mode = "rules";
        if (request.UseAi && ai.IsConfigured && ranked.Count > 0)
        {
            try
            {
                var shortlist = ranked.Take(40).ToList();
                var ids = await ai.RankAsync(request.Query, shortlist.Select(c => new AiCandidate(c.Id, c.Brand.Name,
                    c.Description, c.Category.ToString(), CouponRanking.Savings(c, request.OrderAmount), c.ExpiryDate)).ToArray(), ct);
                var byId = shortlist.ToDictionary(c => c.Id);
                var safeIds = ids.Where(byId.ContainsKey).Distinct().ToList();
                if (safeIds.Count > 0)
                {
                    ranked = safeIds.Select(id => byId[id]).Concat(ranked.Where(c => !safeIds.Contains(c.Id))).ToList();
                    mode = "ai";
                }
                else mode = "rules-fallback";
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or TaskCanceledException or InvalidOperationException or KeyNotFoundException)
            {
                ct.ThrowIfCancellationRequested();
                logger.LogWarning("AI ranking unavailable ({ErrorType}); using deterministic ranking.", ex.GetType().Name);
                mode = "rules-fallback";
            }
        }
        else if (request.UseAi) mode = "rules-fallback";
        return new RecommendationResult(mode, ranked.Where(c => CouponRanking.Eligible(c, DateTime.UtcNow, request.OrderAmount))
            .Take(request.Limit).Select(c => new RankedCoupon(c, CouponRanking.Savings(c, request.OrderAmount),
                preferences.Contains(c.Category.ToString()) ? "Matches a preferred category" : saved.Contains(c.Id) ? "Saved by you" :
                "Available offer; check merchant terms before using")).ToArray());
    }
}
