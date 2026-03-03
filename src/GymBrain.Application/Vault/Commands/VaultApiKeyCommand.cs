using MediatR;

namespace GymBrain.Application.Vault.Commands;

public sealed record VaultApiKeyCommand(
    Guid UserId,
    string Provider,
    string ApiKey,
    string? Model = null) : IRequest<VaultApiKeyResponse>;

public sealed record VaultApiKeyResponse(string Message);
