namespace GymBrain.Application.Common.Interfaces;

/// <summary>
/// Factory that resolves the correct ILlmProvider based on the user's stored provider name.
/// </summary>
public interface ILlmProviderFactory
{
    ILlmProvider GetProvider(string providerName);
}
