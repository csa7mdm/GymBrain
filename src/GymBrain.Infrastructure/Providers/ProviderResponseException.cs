namespace GymBrain.Infrastructure.Providers;

/// <summary>A safe, user-facing description of a provider failure.</summary>
public sealed class ProviderResponseException(string message) : Exception(message);
