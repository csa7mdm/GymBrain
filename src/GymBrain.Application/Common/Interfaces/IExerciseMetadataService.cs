namespace GymBrain.Application.Common.Interfaces;

public interface IExerciseMetadataService
{
    Task<ExerciseMetadata?> GetMetadataAsync(string exerciseName, CancellationToken ct = default);
}

public record ExerciseMetadata(
    string Id,
    string Name,
    string GifUrl,
    string Target,
    string BodyPart,
    string Equipment,
    string[] SecondaryMuscles,
    string[] Instructions);
