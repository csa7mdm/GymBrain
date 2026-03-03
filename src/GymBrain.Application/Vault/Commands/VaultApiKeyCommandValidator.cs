using FluentValidation;
using GymBrain.Application.Common;

namespace GymBrain.Application.Vault.Commands;

public sealed class VaultApiKeyCommandValidator : AbstractValidator<VaultApiKeyCommand>
{
    public VaultApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Provider)
            .NotEmpty()
            .Must(p => LlmModelCatalog.SupportedProviders.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Provider must be one of: {string.Join(", ", LlmModelCatalog.SupportedProviders)}.");
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("API key appears invalid.");
        RuleFor(x => x.Model)
            .Must((cmd, model) =>
                string.IsNullOrEmpty(model) || LlmModelCatalog.IsValidModel(cmd.Provider, model))
            .WithMessage("Model is not available for the selected provider.");
    }
}
