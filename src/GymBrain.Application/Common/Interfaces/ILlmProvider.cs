namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// Strategy pattern interface for LLM providers (OpenAI, Anthropic, etc.).
/// Accepts a system prompt and user message, returns raw JSON string.
/// Token-conscious: prompts are pre-compressed by SystemPromptFactory.
/// </summary>
public interface ILlmProvider
{
    /// <param name="apiKey">Decrypted BYO-API key (never logged).</param>
    /// <param name="systemPrompt">Pre-compressed system prompt from SystemPromptFactory.</param>
    /// <param name="userMessage">User-facing request (e.g. "Generate a push workout").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Raw JSON mega-payload string from the LLM.</returns>
    Task<string> ChatCompletionAsync(string apiKey, string systemPrompt, string userMessage, CancellationToken ct = default);
}
