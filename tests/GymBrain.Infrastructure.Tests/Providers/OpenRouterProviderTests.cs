using System.Net;
using System.Text;
using GymBrain.Infrastructure.Providers;

namespace GymBrain.Infrastructure.Tests.Providers;

public sealed class OpenRouterProviderTests
{
    [Fact]
    public async Task SuccessfulCompletionReturnsContent()
    {
        using var http = Client(HttpStatusCode.OK,
            """{"choices":[{"message":{"content":"{\"components\":[]}"}}]}""");
        var result = await new OpenRouterProvider(http).ChatCompletionAsync(
            "test-key", "example:free", "system", "user");
        Assert.Equal("{" + "\"components\":[]}", result);
    }

    [Theory]
    [InlineData("{\"error\":{\"message\":\"private provider detail\"}}")]
    [InlineData("{\"choices\":[]}")]
    [InlineData("{\"choices\":[{\"message\":{\"content\":null}}]}")]
    public async Task Http200WithoutUsableAnswerReturnsSafeProviderError(string body)
    {
        using var http = Client(HttpStatusCode.OK, body);
        var error = await Assert.ThrowsAsync<ProviderResponseException>(() =>
            new OpenRouterProvider(http).ChatCompletionAsync("test-key", "example:free", "system", "user"));
        Assert.Contains("another current model", error.Message);
        Assert.DoesNotContain("private provider detail", error.Message);
    }

    [Fact]
    public async Task Http200RateLimitEnvelopeGetsActionableError()
    {
        using var http = Client(HttpStatusCode.OK,
            """{"error":{"code":429,"message":"private provider detail"}}""");
        var error = await Assert.ThrowsAsync<ProviderResponseException>(() =>
            new OpenRouterProvider(http).ChatCompletionAsync("test-key", "example:free", "system", "user"));
        Assert.Contains("429", error.Message);
        Assert.DoesNotContain("private provider detail", error.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "saved API key")]
    [InlineData(HttpStatusCode.TooManyRequests, "429")]
    [InlineData(HttpStatusCode.NotFound, "404")]
    public async Task ProviderFailuresDoNotExposeResponseBody(HttpStatusCode status, string expected)
    {
        using var http = Client(status, "private provider detail");
        var error = await Assert.ThrowsAsync<ProviderResponseException>(() =>
            new OpenRouterProvider(http).ChatCompletionAsync("test-key", "example:free", "system", "user"));
        Assert.Contains(expected, error.Message);
        Assert.DoesNotContain("private provider detail", error.Message);
    }

    private static HttpClient Client(HttpStatusCode status, string body) => new(new Handler(_ =>
        new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") }));

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(send(request));
    }
}
