using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GymBrain.Application.Common.Interfaces;

namespace GymBrain.Infrastructure.Providers;

/// <summary>
/// OpenAI LLM provider using gpt-4o-mini for token efficiency.
/// JSON mode enforced to guarantee parseable SDUI mega-payloads.
/// API key is the user's decrypted BYO key — never logged.
/// </summary>
public sealed class OpenAiProvider(HttpClient httpClient) : ILlmProvider
{
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";
    private const string Model = "gpt-4o-mini";

    public async Task<string> ChatCompletionAsync(
        string apiKey,
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = Model,
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
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? throw new InvalidOperationException("LLM returned empty content.");
    }
}
