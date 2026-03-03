using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

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
}
