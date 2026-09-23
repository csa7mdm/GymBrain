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

        using var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new ProviderResponseException(response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden =>
                    "OpenRouter rejected the saved API key or its permissions. Update the connection in Vault.",
                System.Net.HttpStatusCode.TooManyRequests =>
                    "OpenRouter is limiting this model right now (429). Wait or choose another model in Vault.",
                System.Net.HttpStatusCode.NotFound =>
                    "The selected OpenRouter model is unavailable (404). Load the latest models in Vault.",
                _ => "OpenRouter could not complete the request. Try again or choose another model in Vault."
            });
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("code", out var code))
            {
                var codeText = code.ValueKind switch
                {
                    JsonValueKind.Number => code.GetRawText(),
                    JsonValueKind.String => code.GetString(),
                    _ => null
                };
                if (codeText is "429" or "rate_limit_exceeded")
                    throw new ProviderResponseException("OpenRouter is limiting this model right now (429). Wait or choose another model in Vault.");
                if (codeText is "404" or "model_not_found")
                    throw new ProviderResponseException("The selected OpenRouter model is unavailable (404). Load the latest models in Vault.");
            }
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0 &&
                choices[0].ValueKind == JsonValueKind.Object &&
                choices[0].TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.Object &&
                message.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(content.GetString()))
            {
                if (choices[0].TryGetProperty("finish_reason", out var finishReason) &&
                    finishReason.ValueKind == JsonValueKind.String &&
                    finishReason.GetString() == "length")
                    throw new ProviderResponseException(
                        "The selected OpenRouter model ran out of output space before finishing. Choose another model in Vault and retry.");
                return content.GetString()!;
            }
        }
        catch (JsonException) { /* A successful HTTP status is not proof of usable model output. */ }

        // Never expose the provider body; it may contain private request details.
        throw new ProviderResponseException(
            "OpenRouter did not return a usable answer for this model. Choose another current model in Vault and retry.");
    }

    public Task<IEnumerable<string>> GetAvailableModelsAsync(string apiKey, CancellationToken ct = default)
        => LiveModelDiscovery.FetchAsync(httpClient, "openrouter", apiKey, ct);

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
