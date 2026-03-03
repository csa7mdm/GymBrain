using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Infrastructure.Providers;

/// <summary>
/// OpenRouter LLM provider. OpenAI-compatible API.
/// Provides access to various models including free ones.
/// JSON mode supported via provider-specific flags or model capabilities.
/// </summary>
public sealed class OpenRouterProvider(HttpClient httpClient) : ILlmProvider
{
    private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";

    public string ProviderName => "openrouter";

    public async Task<string> ChatCompletionAsync(
        string apiKey,
        string model,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        // OpenRouter headers
        httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://gymbrain.ai"); 
        httpClient.DefaultRequestHeaders.Add("X-Title", "GymBrain");

        var payload = new
        {
            model = model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
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
            throw new InvalidOperationException($"OpenRouter API error: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? throw new InvalidOperationException("OpenRouter returned empty content.");
    }
}
