using FluentValidation;

namespace GymBrain.Application.Vault.Commands;

public sealed class VaultApiKeyCommandValidator : AbstractValidator<VaultApiKeyCommand>
{
    private static readonly HashSet<string> AllowedProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "openai", "anthropic"
    };

    public VaultApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Provider)
            .NotEmpty()
            .Must(p => AllowedProviders.Contains(p))
            .WithMessage("Provider must be one of: openai, anthropic.");
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("API key appears invalid.");
    }
}
