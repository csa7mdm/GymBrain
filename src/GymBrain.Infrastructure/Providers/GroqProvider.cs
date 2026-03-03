using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymBrain.Application.Common;
using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Infrastructure.Providers;

/// <summary>
/// Groq LLM provider. OpenAI-compatible API.
/// Best for fast, free inference using Llama models.
/// JSON mode supported. Key is the user's decrypted BYO key.
/// </summary>
public sealed class GroqProvider(HttpClient httpClient) : ILlmProvider
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

    public string ProviderName => "groq";

    public async Task<string> ChatCompletionAsync(
        string apiKey,
        string model,
        string systemPrompt,
        string userMessage,
        bool forceJson = true,
        int maxTokens = 2048,
        CancellationToken ct = default)
    {
        var payload = new Dictionary<string, object>
        {
            ["model"] = model,
            ["messages"] = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
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

        var response = await httpClient.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Groq API error: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? throw new InvalidOperationException("Groq returned empty content.");
    }

    public Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default)
    {
        return Task.FromResult(LlmModelCatalog.GetByProvider("groq").Select(m => m.ModelId));
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
