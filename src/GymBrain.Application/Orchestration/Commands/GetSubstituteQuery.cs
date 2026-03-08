using GymBrain.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GymBrain.Application.Orchestration.Commands;

public record GetSubstituteQuery(Guid ExerciseId, string? UserInjuries)
    : MediatR.IRequest<SubstituteResult>;

public record SubstituteResult(
    string OriginalExerciseId,
    string OriginalExerciseName,
    IReadOnlyList<SubstituteOption> Substitutes,
    string? Message);

public record SubstituteOption(
    string ExerciseId,
    string Name,
    string Equipment,
    string Reason);
