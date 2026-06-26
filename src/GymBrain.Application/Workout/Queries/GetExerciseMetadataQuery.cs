using GymBrain.Application.Common.Interfaces;
using MediatR;

namespace GymBrain.Application.Workout.Queries;

public record GetExerciseMetadataQuery(string Name) : IRequest<ExerciseMetadata?>;

public class GetExerciseMetadataQueryHandler(
    IExerciseMetadataService metadataService,
    ICacheService cache)
    : IRequestHandler<GetExerciseMetadataQuery, ExerciseMetadata?>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(30);

    public async Task<ExerciseMetadata?> Handle(GetExerciseMetadataQuery request, CancellationToken ct)
    {
        var cacheKey = $"ex_metadata:{request.Name.ToLower().Trim().Replace(" ", "_")}";
        
        // 1. Try cache
        var cached = await cache.GetAsync<ExerciseMetadata>(cacheKey, ct);
        if (cached is not null)
            return cached;

        // 2. Fetch from service
        var metadata = await metadataService.GetMetadataAsync(request.Name, ct);
        
        // 3. Save to cache if found
        if (metadata is not null)
        {
            await cache.SetAsync(cacheKey, metadata, CacheTtl, ct);
        }

        return metadata;
    }
}
