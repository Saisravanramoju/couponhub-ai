using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;
using CouponHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CouponHub.Tests;

public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("COUPONHUB_TEST_DB")))
            Skip = "Set COUPONHUB_TEST_DB to a disposable PostgreSQL database to run integration tests.";
    }
}
public sealed class ApiIntegrationTests
{
    private sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("COUPONHUB_TEST_DB"),
                ["AI:ApiKey"] = "", ["AI:Model"] = ""
            }));
        }
    }
    private static async Task<HttpClient> Register(Factory factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/register", new { email = $"test-{Guid.NewGuid():N}@example.com", password = "Integration-Test-Password-482!" });
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.GetProperty("accessToken").GetString());
        return client;
    }
    [DatabaseFact]
    public async Task AccountsCannotReadSaveModifyOrRecommendOtherUsersPrivateCoupons()
    {
        await using var factory = new Factory();
        Guid brandId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
            var brand = new Brand($"Test-{Guid.NewGuid():N}", BrandCategory.Shopping);
            db.Add(brand); await db.SaveChangesAsync(); brandId = brand.Id;
        }
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/coupons")).StatusCode);
        using var owner = await Register(factory); using var other = await Register(factory);
        var create = await owner.PostAsJsonAsync("/api/coupons", new {
            brandId, couponCode = "PRIVATE20", description = "Private offer", category = "Shopping", discountType = "Percentage",
            discountValue = 20, minimumOrderAmount = 100, maximumDiscount = 5,
            expiryDate = DateTime.UtcNow.AddDays(2), couponSource = "Manual"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var coupon = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = coupon.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync($"/api/coupons/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/coupons/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsync($"/api/saved/{id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/coupons/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.PostAsync($"/api/coupons/{id}/publish", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.PutAsync($"/api/saved/{id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.PutAsync($"/api/saved/{id}", null)).StatusCode);
        var recs = await other.PostAsJsonAsync("/api/recommendations", new { query = "Private", orderAmount = 500, useAi = true });
        recs.EnsureSuccessStatusCode();
        Assert.DoesNotContain(id.ToString(), await recs.Content.ReadAsStringAsync());
        var belowMin = await owner.PostAsJsonAsync("/api/recommendations", new { query = "Private", orderAmount = 50 });
        belowMin.EnsureSuccessStatusCode();
        Assert.DoesNotContain(id.ToString(), await belowMin.Content.ReadAsStringAsync());
        await owner.PostAsync("/api/account/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await owner.GetAsync("/api/account")).StatusCode);
    }
}
