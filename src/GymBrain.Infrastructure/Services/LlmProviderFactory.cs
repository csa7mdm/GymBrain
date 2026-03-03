using GymBrain.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GymBrain.Infrastructure.Services;

/// <summary>
/// Resolves ILlmProvider based on the provider name using the DI container.
/// </summary>
public sealed class LlmProviderFactory(IServiceProvider serviceProvider) : ILlmProviderFactory
{
    public ILlmProvider GetProvider(string providerName)
    {
        var providers = serviceProvider.GetServices<ILlmProvider>();
        
        return providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"LLM provider '{providerName}' is not registered.");
    }
}
