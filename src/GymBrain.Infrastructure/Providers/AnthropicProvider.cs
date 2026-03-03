using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Infrastructure.Providers;

/// <summary>
/// Anthropic LLM provider (Claude).
/// Uses Anthropic-specific API.
/// Key is the user's decrypted BYO key.
/// </summary>
public sealed class AnthropicProvider(HttpClient httpClient) : ILlmProvider
{
    private const string Endpoint = "https://api.anthropic.com/v1/messages";
    private const string Version = "2023-06-01";

    public string ProviderName => "anthropic";

    public async Task<string> ChatCompletionAsync(
        string apiKey,
        string model,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", Version);

        var payload = new
        {
            model = model,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            },
            max_tokens = 2048,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(Endpoint, content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Anthropic API error: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? throw new InvalidOperationException("Anthropic returned empty content.");
    }
}
