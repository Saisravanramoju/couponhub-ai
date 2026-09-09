using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using CouponHub.Api.Security;
using CouponHub.Application.Personalization;
using CouponHub.Domain.Enums;
using CouponHub.Infrastructure.Personalization;
using CouponHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Api.Controllers;

[ApiController, Route("api/account")]
public sealed class AccountsController(ApplicationDbContext db, ICurrentUser user) : ControllerBase
{
    public sealed record Credentials([Required, EmailAddress, StringLength(254)] string Email,
        [Required, StringLength(128, MinimumLength = 12)] string Password);
    public sealed record Preferences([Required, MaxLength(10)] CouponCategory[] Categories);
    private static readonly PasswordHasher<Account> Hasher = new();
    // Use the same password work for unknown addresses to reduce timing differences.
    private static readonly string DummyHash = Hasher.HashPassword(new Account(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

    [AllowAnonymous, HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(Credentials request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Set<Account>().AnyAsync(a => a.Email == email, ct))
            return Conflict(new { message = "Unable to register this address. Try signing in." });
        var account = new Account { Email = email };
        account.PasswordHash = Hasher.HashPassword(account, request.Password);
        db.Add(account);
        await db.SaveChangesAsync(ct);
        return Ok(await CreateSession(account, ct));
    }

    [AllowAnonymous, HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(Credentials request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await db.Set<Account>().SingleOrDefaultAsync(a => a.Email == email, ct);
        var result = Hasher.VerifyHashedPassword(account ?? new Account(), account?.PasswordHash ?? DummyHash, request.Password);
        if (account is null || result == PasswordVerificationResult.Failed) return Unauthorized(new { message = "Invalid email or password." });
        if (result == PasswordVerificationResult.SuccessRehashNeeded) account.PasswordHash = Hasher.HashPassword(account, request.Password);
        return Ok(await CreateSession(account, ct));
    }

    private async Task<object> CreateSession(Account account, CancellationToken ct)
    {
        await db.Set<Session>().Where(s => s.AccountId == account.Id && s.ExpiresAt <= DateTime.UtcNow).ExecuteDeleteAsync(ct);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTime.UtcNow.AddDays(7);
        db.Add(new Session { AccountId = account.Id, TokenHash = SessionAuthentication.Hash(token), ExpiresAt = expiresAt });
        await db.SaveChangesAsync(ct);
        return new { accessToken = token, expiresAt, account.Id, account.Email, account.IsAdmin };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Headers.Authorization.ToString()[7..];
        var hash = SessionAuthentication.Hash(token);
        await db.Set<Session>().Where(s => s.TokenHash == hash && s.AccountId == user.Id).ExecuteDeleteAsync(ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Me(CancellationToken ct) => Ok(await db.Set<Account>().Where(a => a.Id == user.Id)
        .Select(a => new { a.Id, a.Email, a.IsAdmin, a.Categories }).SingleAsync(ct));

    [HttpPut("preferences")]
    public async Task<IActionResult> SavePreferences(Preferences request, CancellationToken ct)
    {
        if (request.Categories.Any(c => !Enum.IsDefined(c))) return BadRequest();
        var account = await db.Set<Account>().SingleAsync(a => a.Id == user.Id, ct);
        account.Categories = request.Categories.Select(c => c.ToString()).Distinct().ToArray();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
