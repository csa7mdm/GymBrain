using System.Net.Http.Headers;
using System.Text.Json;

namespace GymBrain.Infrastructure.Providers;

public static class LiveModelDiscovery
{
    public static async Task<IEnumerable<string>> FetchAsync(HttpClient client, string provider, string key, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        var baseUrl = provider switch {
            "openrouter" => "https://openrouter.ai/api/v1",
            "groq" => "https://api.groq.com/openai/v1",
            "openai" => "https://api.openai.com/v1",
            _ => throw new ArgumentException("Unsupported provider.")
        };
        async Task<string> Get(string path)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            using var response = await client.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException((int)response.StatusCode switch {
                    401 or 403 => "The provider rejected this key or its permissions. Check the selected provider and API key.",
                    429 => "The provider is rate limiting requests. Wait and refresh the model list.",
                    _ => "The provider model service is unavailable. Please retry later."
                });
            return await response.Content.ReadAsStringAsync(timeout.Token);
        }
        try
        {
            // The public OpenRouter catalog alone does not authenticate the key.
            if (provider == "openrouter") await Get("/key");
            using var doc = JsonDocument.Parse(await Get("/models"));
            return doc.RootElement.GetProperty("data").EnumerateArray()
                .Where(m => Compatible(m, provider))
                .OrderByDescending(m => m.TryGetProperty("created", out var created) && created.TryGetInt64(out var value) ? value : 0)
                .Select(m => m.GetProperty("id").GetString()!).Distinct().ToArray();
        }
        catch (HttpRequestException) { throw new InvalidOperationException("Could not reach the provider. Please retry."); }
        catch (JsonException) { throw new InvalidOperationException("The provider returned an unreadable model list. Please retry."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new InvalidOperationException("Model loading timed out. Please retry."); }
    }

    private static bool Compatible(JsonElement model, string provider)
    {
        var id = model.GetProperty("id").GetString() ?? "";
        if (provider == "openrouter")
            return id.EndsWith(":free", StringComparison.Ordinal) &&
                model.TryGetProperty("supported_parameters", out var parameters) &&
                parameters.EnumerateArray().Any(p => p.GetString() == "response_format");
        if (provider == "groq") return !id.Contains("whisper") && !id.Contains("tts") && !id.Contains("guard") && !id.Contains("safeguard") && !id.Contains("compound");
        return id.StartsWith("gpt-", StringComparison.Ordinal) &&
            !new[] { "audio", "realtime", "transcribe", "image", "search", "codex", "instruct" }.Any(id.Contains);
    }
}
