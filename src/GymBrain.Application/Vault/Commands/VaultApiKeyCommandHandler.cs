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

        var provider = llmProviderFactory.GetProvider(request.Provider);
        var available = (await provider.GetAvailableModelsAsync(request.ApiKey, ct)).ToArray();
        var workingModel = request.Model ?? available.FirstOrDefault();
        if (workingModel == null || !available.Contains(workingModel, StringComparer.Ordinal))
            throw new InvalidOperationException("This model is no longer available for this provider. Refresh the model list and choose again.");

        var encrypted = vaultService.Encrypt(request.ApiKey);
        user.VaultApiKey(encrypted, request.Provider, workingModel);

        await db.SaveChangesAsync(ct);
        
        var message = $"Key verified and saved for {request.Provider} ({workingModel}). Model access and provider quotas still apply when generating.";

        return new VaultApiKeyResponse(message);
    }
}
