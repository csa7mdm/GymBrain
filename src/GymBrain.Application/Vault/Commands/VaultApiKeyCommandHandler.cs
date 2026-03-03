using GymBrain.Application.Common;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Vault.Commands;

public sealed class VaultApiKeyCommandHandler(
    IApplicationDbContext db,
    IVaultService vaultService,
    ILlmProviderFactory llmProviderFactory)
    : IRequestHandler<VaultApiKeyCommand, VaultApiKeyResponse>
{
    public async Task<VaultApiKeyResponse> Handle(VaultApiKeyCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var model = request.Model ?? LlmModelCatalog.GetDefaultModel(request.Provider);
        
        // --- Pre-flight Health Check and Automatic Fallback ---
        var provider = llmProviderFactory.GetProvider(request.Provider);
        var modelsToTry = new List<string> { model };
        
        // Add other free models from catalog as candidates if this is a free-tier test
        var otherFreeModels = LlmModelCatalog.GetByProvider(request.Provider)
            .Where(m => m.IsFree && m.ModelId != model)
            .Select(m => m.ModelId);
        modelsToTry.AddRange(otherFreeModels);

        string workingModel = string.Empty;
        foreach (var m in modelsToTry)
        {
            if (await provider.CheckHealthAsync(request.ApiKey, m, ct))
            {
                workingModel = m;
                break;
            }
        }

        if (string.IsNullOrEmpty(workingModel))
        {
            throw new InvalidOperationException(
                $"Authentication failed or all tested models are currently unavailable for {request.Provider}. Please check your API key.");
        }

        var encrypted = vaultService.Encrypt(request.ApiKey);
        user.VaultApiKey(encrypted, request.Provider, workingModel);

        await db.SaveChangesAsync(ct);
        
        var message = workingModel == model 
            ? $"API key verified and vaulted for {request.Provider} ({workingModel})."
            : $"API key verified! '{model}' was unavailable, so we switched you to '{workingModel}'.";

        return new VaultApiKeyResponse(message);
    }
}
