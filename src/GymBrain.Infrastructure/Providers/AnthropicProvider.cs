using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymBrain.Application.Common;
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
        bool forceJson = true,
        int maxTokens = 2048,
        CancellationToken ct = default)
    {
        var payload = new
        {
            model = model,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            },
            max_tokens = maxTokens,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", Version);

        var response = await httpClient.SendAsync(request, ct);
        
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

    public Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default)
    {
        // Anthropic models are stable and don't change often
        return Task.FromResult(LlmModelCatalog.GetByProvider("anthropic").Select(m => m.ModelId));
    }

    public async Task<bool> CheckHealthAsync(string apiKey, string model, CancellationToken ct = default)
    {
        try
        {
            await ChatCompletionAsync(apiKey, model, "hi", "hi", forceJson: false, maxTokens: 1, ct: ct);
            return true;
        }
        catch { return false; }
    }
}
