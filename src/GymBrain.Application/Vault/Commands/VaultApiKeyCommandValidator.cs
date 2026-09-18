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
            .Must(p => new[] { "openai", "groq", "openrouter" }.Contains(p, StringComparer.Ordinal))
            .WithMessage($"Provider must be one of: {string.Join(", ", LlmModelCatalog.SupportedProviders)}.");
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("API key appears invalid.");
        RuleFor(x => x.Model)
            .Must((cmd, model) =>
                string.IsNullOrEmpty(model) || (model.Length <= 256 && !model.Any(char.IsControl)))
            .WithMessage("Model is not available for the selected provider.");
    }
}
