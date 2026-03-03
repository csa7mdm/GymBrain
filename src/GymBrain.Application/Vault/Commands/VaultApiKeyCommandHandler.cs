using GymBrain.Application.Common;
using GymBrain.Application.Common.Interfaces;
using GymBrain.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymBrain.Application.Vault.Commands;

public sealed class VaultApiKeyCommandHandler(
    IApplicationDbContext db,
    IVaultService vaultService)
    : IRequestHandler<VaultApiKeyCommand, VaultApiKeyResponse>
{
    public async Task<VaultApiKeyResponse> Handle(VaultApiKeyCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var encrypted = vaultService.Encrypt(request.ApiKey);
        var model = request.Model ?? LlmModelCatalog.GetDefaultModel(request.Provider);
        user.VaultApiKey(encrypted, request.Provider, model);

        await db.SaveChangesAsync(ct);
        return new VaultApiKeyResponse($"API key securely vaulted for {request.Provider} ({model}).");
    }
}
