namespace GymBrain.Domain.Common;

public class NarrativeChapter
{
    public int Number { get; set; }
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
    public int MinWorkouts { get; set; }
    public int? MaxWorkouts { get; set; }
    public string UnlockMessage { get; set; } = null!;
}

public static class NarrativeProgress
{
    private static readonly List<NarrativeChapter> Chapters = new()
    {
        new NarrativeChapter { 
            Number = 0, 
            Title = "Prologue", 
            Subtitle = "The First Step", 
            MinWorkouts = 0, 
            MaxWorkouts = 0,
            UnlockMessage = "Your story begins. Today is the day everything changes."
        },
        new NarrativeChapter { 
            Number = 1, 
            Title = "Chapter 1", 
            Subtitle = "Awakening", 
            MinWorkouts = 1, 
            MaxWorkouts = 2,
            UnlockMessage = "The fire is lit. You've proven you can show up."
        },
        new NarrativeChapter { 
            Number = 2, 
            Title = "Chapter 2", 
            Subtitle = "The Grind", 
            MinWorkouts = 3, 
            MaxWorkouts = 5,
            UnlockMessage = "Habits are forming. The gym is no longer a stranger."
        },
        new NarrativeChapter { 
            Number = 3, 
            Title = "Chapter 3", 
            Subtitle = "Transformation", 
            MinWorkouts = 6, 
            MaxWorkouts = 9,
            UnlockMessage = "You can see it. Your body is starting to reflect your will."
        },
        new NarrativeChapter { 
            Number = 4, 
            Title = "Chapter 4", 
            Subtitle = "Beyond Limits", 
            MinWorkouts = 10, 
            MaxWorkouts = 14,
            UnlockMessage = "You are stronger than you ever thought possible."
        },
        new NarrativeChapter { 
            Number = 5, 
            Title = "Chapter 5", 
            Subtitle = "Legacy", 
            MinWorkouts = 15, 
            MaxWorkouts = null,
            UnlockMessage = "You are an inspiration. This is no longer a phase; it's who you are."
        }
    };

    public static NarrativeChapter GetChapter(int workoutsCompleted)
    {
        return Chapters.FirstOrDefault(c => 
            workoutsCompleted >= c.MinWorkouts && 
            (c.MaxWorkouts == null || workoutsCompleted <= c.MaxWorkouts)) 
            ?? Chapters.Last();
    }
}
