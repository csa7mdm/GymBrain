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
        bool forceJson = true,
        int maxTokens = 2048,
        CancellationToken ct = default)
    {
        var messages = new List<object>();

        // Gemma compatibility: many free endpoints don't support 'system' role
        if (model.Contains("gemma", StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(new { role = "user", content = $"[SYSTEM INSTRUCTION]\n{systemPrompt}\n\n[USER REQUEST]\n{userMessage}" });
        }
        else
        {
            messages.Add(new { role = "system", content = systemPrompt });
            messages.Add(new { role = "user", content = userMessage });
        }

        var payload = new Dictionary<string, object>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["max_tokens"] = maxTokens,
            ["temperature"] = 0.7
        };

        if (forceJson)
        {
            payload["response_format"] = new { type = "json_object" };
        }

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        // OpenRouter specific headers
        request.Headers.Add("HTTP-Referer", "https://gymbrain.ai");
        request.Headers.Add("X-Title", "GymBrain");

        var response = await httpClient.SendAsync(request, ct);

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

    public async Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://openrouter.ai/api/v1/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<string>();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        
        return doc.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(m => m.GetProperty("id").GetString()!)
            .Where(id => id.EndsWith(":free"))
            .ToList(); // Materialize before JsonDocument is disposed
    }

    public async Task<bool> CheckHealthAsync(string apiKey, string model, CancellationToken ct = default)
    {
        try
        {
            // Minimal request to verify key and model. 
            // We set forceJson=false because some free models on OpenRouter 
            // fail with 400 if response_format is requested but not supported.
            // Also set maxTokens very low for speed.
            await ChatCompletionAsync(apiKey, model, "hi", "hi", forceJson: false, maxTokens: 1, ct: ct);
            return true;
        }
        catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("403") || ex.Message.Contains("429") || ex.Message.Contains("404"))
        {
            return false;
        }
    }
}
