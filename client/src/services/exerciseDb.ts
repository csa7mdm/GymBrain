const EXERCISE_DB_BASE = 'https://exercisedb-api.vercel.app/api/v1';

export interface ExerciseDbItem {
  exerciseId: string;
  name: string;
  gifUrl: string;
  targetMuscles: string[];
  bodyParts: string[];
  equipments: string[];
  secondaryMuscles: string[];
  instructions: string[];
}

const exerciseCache = new Map<string, ExerciseDbItem>();

export async function searchExercise(name: string): Promise<ExerciseDbItem | null> {
  const cacheKey = name.toLowerCase().trim();
  if (exerciseCache.has(cacheKey)) return exerciseCache.get(cacheKey)!;
  try {
    const searchTerm = encodeURIComponent(name.toLowerCase().replace(/[^a-z0-9 ]/g, '').trim());
    const res = await fetch(EXERCISE_DB_BASE + '/exercises?search=' + searchTerm + '&limit=5');
    if (!res.ok) return null;
    const json = await res.json();
    const exercises: ExerciseDbItem[] = json.data || [];
    if (exercises.length === 0) return null;
    let best = exercises[0];
    let bestScore = 0;
    for (const ex of exercises) {
      const exName = ex.name.toLowerCase();
      if (exName === cacheKey) { best = ex; break; }
      const score = cacheKey.split(' ').filter(word => exName.includes(word)).length;
      if (score > bestScore) { bestScore = score; best = ex; }
    }
    exerciseCache.set(cacheKey, best);
    return best;
  } catch { return null; }
}