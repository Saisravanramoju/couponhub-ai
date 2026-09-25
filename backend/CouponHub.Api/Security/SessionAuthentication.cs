using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using CouponHub.Application.Personalization;
using CouponHub.Infrastructure.Personalization;
using CouponHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CouponHub.Api.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? Id => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public bool IsAdmin => accessor.HttpContext?.User.IsInRole("Admin") == true;
}

public sealed class SessionAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, ApplicationDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Session";
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
        var token = header[7..];
        if (token.Length != 64) return AuthenticateResult.Fail("Invalid session.");
        var hash = Hash(token);
        var account = await (from s in db.Set<Session>().AsNoTracking()
            join a in db.Set<Account>().AsNoTracking() on s.AccountId equals a.Id
            where s.TokenHash == hash && s.ExpiresAt > DateTime.UtcNow select a).FirstOrDefaultAsync(Context.RequestAborted);
        if (account is null) return AuthenticateResult.Fail("Session expired or invalid.");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Role, account.IsAdmin ? "Admin" : "User") };
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(
            new ClaimsIdentity(claims, SchemeName)), SchemeName));
    }
}
