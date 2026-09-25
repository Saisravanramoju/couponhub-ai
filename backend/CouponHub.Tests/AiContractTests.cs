using System.Net;
using System.Text;
using CouponHub.Application.Personalization;
using CouponHub.Infrastructure.Personalization;
using Microsoft.Extensions.Configuration;

namespace CouponHub.Tests;

public sealed class AiContractTests
{
    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
    [Fact] public async Task UnknownAndDuplicateAiIdsAreDiscarded()
    {
        var allowed = Guid.NewGuid(); var unknown = Guid.NewGuid();
        var content = System.Text.Json.JsonSerializer.Serialize(new { ids = new[] { unknown, allowed, allowed } });
        var body = System.Text.Json.JsonSerializer.Serialize(new { choices = new[] { new { finish_reason = "stop", message = new { content } } } });
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["AI:ApiKey"] = "test-only", ["AI:Model"] = "test-model" }).Build();
        var service = new OpenAiCouponService(new HttpClient(new StubHandler(body)), config);
        var result = await service.RankAsync("food", [new AiCandidate(allowed, "Brand", "Offer", "Food", 20, null)], default);
        Assert.Equal(new[] { allowed }, result);
    }
    [Fact] public void AiRequiresBothModelAndKey()
    {
        var service = new OpenAiCouponService(new HttpClient(), new ConfigurationBuilder().Build());
        Assert.False(service.IsConfigured);
    }
}
