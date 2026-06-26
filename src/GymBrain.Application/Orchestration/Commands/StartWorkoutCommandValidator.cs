using FluentValidation;

namespace GymBrain.Application.Orchestration.Commands;

public sealed class StartWorkoutCommandValidator : AbstractValidator<StartWorkoutCommand>
{
    public StartWorkoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.WorkoutFocus)
            .MaximumLength(100).WithMessage("Workout focus description is too long (max 100 characters).")
            .Must(f => string.IsNullOrEmpty(f) || !f.Contains("<script")).WithMessage("Invalid characters in workout focus.");
    }
}
