using System.Net;
using System.Text;
using GymBrain.Infrastructure.Providers;

namespace GymBrain.Infrastructure.Tests.Providers;
public class LiveModelDiscoveryTests
{
    [Fact]
    public async Task OpenRouterAuthenticatesKeyAndReturnsCurrentFreeJsonModels()
    {
        var paths = new List<string>();
        using var http = new HttpClient(new Handler(request => {
            paths.Add(request.RequestUri!.AbsolutePath);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(
                request.RequestUri.AbsolutePath.EndsWith("/key") ? "{}" : """
                {"data":[{"id":"old:free","created":1,"supported_parameters":["response_format"]},
                {"id":"new:free","created":2,"supported_parameters":["response_format"]},
                {"id":"paid","created":3,"supported_parameters":["response_format"]},
                {"id":"no-json:free","created":4,"supported_parameters":[]}]}
                """, Encoding.UTF8, "application/json") };
        }));
        Assert.Equal(new[] { "new:free", "old:free" }, await LiveModelDiscovery.FetchAsync(http, "openrouter", "test-secret", default));
        Assert.Equal(new[] { "/api/v1/key", "/api/v1/models" }, paths);
    }
    [Theory]
    [InlineData(401, "rejected")]
    [InlineData(429, "rate limiting")]
    [InlineData(503, "unavailable")]
    public async Task ErrorsAreDistinctAndNeverExposeProviderBody(int code, string expected)
    {
        using var http = new HttpClient(new Handler(_ => new HttpResponseMessage((HttpStatusCode)code) { Content = new StringContent("sensitive-provider-body") }));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => LiveModelDiscovery.FetchAsync(http, "groq", "test-secret", default));
        Assert.Contains(expected, error.Message);
        Assert.DoesNotContain("sensitive", error.Message);
    }
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(send(request));
    }
}
