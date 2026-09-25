using System.ComponentModel.DataAnnotations;
using CouponHub.Api.Contracts.Responses;
using CouponHub.Application.Personalization;
using CouponHub.Domain.Enums;
using CouponHub.Infrastructure.Personalization;
using CouponHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Api.Controllers;

[ApiController, Route("api")]
public sealed class DiscoveryController(ApplicationDbContext db, ICurrentUser user,
    RecommendationService recommendations, ICouponAi ai) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery, StringLength(200)] string q = "", [FromQuery] CouponCategory? category = null,
        [FromQuery, Range(1, 100000)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 30, CancellationToken ct = default)
    {
        if (category.HasValue && !Enum.IsDefined(category.Value)) return BadRequest();
        var now = DateTime.UtcNow;
        var query = db.Coupons.AsNoTracking().Include(c => c.Brand)
            .Where(c => c.IsActive && c.Brand.IsActive && (c.ExpiryDate == null || c.ExpiryDate > now));
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c => c.Description.ToLower().Contains(term) || c.Brand.Name.ToLower().Contains(term) || c.CouponCode.ToLower().Contains(term));
        }
        if (category.HasValue) query = query.Where(c => c.Category == category.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { total, page, pageSize, items = items.Select(CouponResponse.FromEntity) });
    }

    [HttpPost("recommendations"), EnableRateLimiting("ai")]
    public async Task<IActionResult> Recommend(RecommendationRequest request, CancellationToken ct)
    {
        if (request.Query is null) return BadRequest();
        var result = await recommendations.Recommend(request, ct);
        return Ok(new { result.Mode, items = result.Items.Select(r => new { coupon = CouponResponse.FromEntity(r.Coupon), r.EstimatedSavings, r.Reason }) });
    }

    public sealed record ImportText([Required, StringLength(12000, MinimumLength = 5)] string Text, bool ConsentToAi);
    [HttpPost("imports/extract"), EnableRateLimiting("ai")]
    public async Task<IActionResult> Extract(ImportText request, CancellationToken ct)
    {
        if (!request.ConsentToAi) return BadRequest(new { message = "Consent is required before sending this text to the AI provider." });
        if (!ai.IsConfigured) return StatusCode(503, new { message = "AI extraction is not configured. Add the coupon manually." });
        try
        {
            var draft = await ai.ExtractAsync(request.Text, ct);
            // Never save AI output automatically: client must review it, choose a known brand,
            // and submit through the same domain validation as manually entered coupons.
            return Ok(new { draft, requiresReview = true });
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or TaskCanceledException or InvalidOperationException or KeyNotFoundException)
        {
            ct.ThrowIfCancellationRequested();
            return StatusCode(503, new { message = "Extraction is temporarily unavailable. Add the coupon manually." });
        }
    }

    [HttpGet("saved")]
    public async Task<IActionResult> Saved(CancellationToken ct)
    {
        var ids = db.Set<SavedCoupon>().Where(s => s.AccountId == user.Id).Select(s => s.CouponId);
        var coupons = await db.Coupons.AsNoTracking().Include(c => c.Brand).Where(c => ids.Contains(c.Id))
            .OrderBy(c => c.ExpiryDate).Take(500).ToListAsync(ct);
        return Ok(coupons.Select(CouponResponse.FromEntity));
    }

    [HttpPut("saved/{id:guid}")]
    public async Task<IActionResult> Save(Guid id, CancellationToken ct)
    {
        if (!await db.Coupons.AnyAsync(c => c.Id == id, ct)) return NotFound();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO saved_coupons (\"AccountId\", \"CouponId\", \"SavedAt\") VALUES ({user.Id!.Value}, {id}, {DateTime.UtcNow}) ON CONFLICT DO NOTHING", ct);
        return NoContent();
    }

    [HttpDelete("saved/{id:guid}")]
    public async Task<IActionResult> Unsave(Guid id, CancellationToken ct)
    {
        await db.Set<SavedCoupon>().Where(s => s.AccountId == user.Id && s.CouponId == id).ExecuteDeleteAsync(ct);
        return NoContent();
    }

    public sealed record TrackRequest([Required] string Kind);
    [HttpPost("coupons/{id:guid}/events")]
    public async Task<IActionResult> Track(Guid id, TrackRequest request, CancellationToken ct)
    {
        if (request.Kind is not ("copy" or "redeem")) return BadRequest();
        if (!await db.Coupons.AnyAsync(c => c.Id == id, ct)) return NotFound();
        db.Add(new CouponEvent { AccountId = user.Id!.Value, CouponId = id, Kind = request.Kind });
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        var now = DateTime.UtcNow; var until = now.AddDays(3);
        var ids = db.Set<SavedCoupon>().Where(s => s.AccountId == user.Id).Select(s => s.CouponId);
        return Ok(await db.Coupons.Where(c => ids.Contains(c.Id) && c.IsActive && c.Brand.IsActive && c.ExpiryDate > now && c.ExpiryDate <= until)
            .OrderBy(c => c.ExpiryDate).Select(c => new { c.Id, c.CouponCode, c.ExpiryDate, message = "Saved coupon expires soon" }).Take(100).ToListAsync(ct));
    }
}
