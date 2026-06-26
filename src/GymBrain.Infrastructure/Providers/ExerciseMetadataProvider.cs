using GymBrain.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace GymBrain.Infrastructure.Providers;

public class ExerciseMetadataProvider(HttpClient httpClient) : IExerciseMetadataService
{
    private const string BaseUrl = "https://musclewiki.com/exercise/";

    public async Task<ExerciseMetadata?> GetMetadataAsync(string exerciseName, CancellationToken ct = default)
    {
        // 1. Generate MuscleWiki Slug
        // e.g. "Dumbbell Incline Bench Press" -> "dumbbell-incline-bench-press"
        var slug = exerciseName.ToLower()
            .Replace(" ", "-")
            .Replace("(", "")
            .Replace(")", "")
            .Trim();

        var url = $"{BaseUrl}{slug}";

        try
        {
            var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var html = await response.Content.ReadAsStringAsync(ct);

            // 2. Extract Video URLs
            // Looking for <video src="...mp4" ...>
            var videoMatches = Regex.Matches(html, "video src=\"(https://media.musclewiki.com/[^\"]+)\"");
            var videos = videoMatches.Select(m => m.Groups[1].Value).Distinct().ToList();
            
            // Prefer the first one or a front view if available
            var videoUrl = videos.FirstOrDefault(v => v.Contains("front")) ?? videos.FirstOrDefault() ?? "";

            // 3. Extract Instructions
            // MuscleWiki usually has an ordered list <ol> with instructions
            var instructions = new List<string>();
            var olMatch = Regex.Match(html, "<ol[^>]*>(.*?)</ol>", RegexOptions.Singleline);
            if (olMatch.Success)
            {
                var liMatches = Regex.Matches(olMatch.Groups[1].Value, "<li>(.*?)</li>", RegexOptions.Singleline);
                foreach (Match li in liMatches)
                {
                    var text = Regex.Replace(li.Groups[1].Value, "<[^>]*>", "").Trim();
                    if (!string.IsNullOrEmpty(text))
                        instructions.Add(text);
                }
            }

            return new ExerciseMetadata(
                slug,
                exerciseName,
                videoUrl, // Note: Frontend expects 'gifUrl' but will play .mp4 in Video component
                "", // Target (Scraping logic for these is complex, keep empty for now)
                "", // BodyPart
                "", // Equipment
                Array.Empty<string>(),
                instructions.ToArray()
            );
        }
        catch
        {
            return null;
        }
    }
}
