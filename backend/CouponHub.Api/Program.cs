using CouponHub.Api.Middleware;
using CouponHub.Api.Security;
using CouponHub.Application;
using CouponHub.Application.Behaviors;
using CouponHub.Application.Personalization;
using CouponHub.Infrastructure;
using CouponHub.Infrastructure.Persistence;
using CouponHub.Infrastructure.Personalization;
using FluentValidation;
using Microsoft.OpenApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CouponHub.Application.DependencyInjection).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CouponHub.Application.DependencyInjection).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddHttpClient<ICouponAi, OpenAiCouponService>(client => client.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddAuthentication(SessionAuthentication.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthentication>(SessionAuthentication.SchemeName, _ => {});
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("ai", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Opaque session token",
        In = ParameterLocation.Header,
        Description = "Enter the accessToken returned by login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 128 * 1024);
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    return;
}
if (args.Contains("--create-admin"))
{
    var email = Environment.GetEnvironmentVariable("COUPONHUB_ADMIN_EMAIL")?.Trim().ToLowerInvariant();
    var password = Environment.GetEnvironmentVariable("COUPONHUB_ADMIN_PASSWORD");
    if (email is null || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email) ||
        email.Length > 254 || password is null || password.Length is < 12 or > 128)
        throw new InvalidOperationException("Set COUPONHUB_ADMIN_EMAIL and COUPONHUB_ADMIN_PASSWORD (12–128 characters).");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (await db.Set<Account>().AnyAsync(a => a.Email == email))
        throw new InvalidOperationException("An account with this email already exists; no permissions were changed.");
    var account = new Account { Email = email, IsAdmin = true };
    account.PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<Account>().HashPassword(account, password);
    db.Add(account); await db.SaveChangesAsync();
    return;
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
else { app.UseHsts(); app.UseHttpsRedirection(); }
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" })).AllowAnonymous();
app.MapGet("/health/ready", async (ApplicationDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503)).AllowAnonymous();
app.MapControllers();
app.Run();
public partial class Program { }
