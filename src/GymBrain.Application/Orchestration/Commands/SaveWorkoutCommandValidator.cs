using FluentValidation;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class SaveWorkoutCommandValidator : AbstractValidator<SaveWorkoutCommand>
{
    public SaveWorkoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.PayloadJson)
            .NotEmpty().WithMessage("Workout data is missing.")
            .Must(p => p.Trim().StartsWith("{") || p.Trim().StartsWith("[")).WithMessage("Invalid workout data format.");
    }
}
