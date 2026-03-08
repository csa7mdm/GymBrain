const RAPIDAPI_KEY = import.meta.env.VITE_RAPIDAPI_KEY || '';
const EXERCISE_DB_BASE = 'https://exercisedb.p.rapidapi.com';

// Interface adapted for RapidAPI response format
export interface ExerciseDbItem {
  id: string;
  name: string;
  gifUrl: string;
  target: string;
  bodyPart: string;
  equipment: string;
  secondaryMuscles: string[];
  instructions: string[];
}

const exerciseCache = new Map<string, ExerciseDbItem>();

export async function searchExercise(name: string): Promise<ExerciseDbItem | null> {
  const cacheKey = name.toLowerCase().trim();

  // 1. Check memory cache
  if (exerciseCache.has(cacheKey)) return exerciseCache.get(cacheKey)!;

  // 2. Check localStorage cache
  try {
    const local = localStorage.getItem(`gymbrain_exdb_${cacheKey}`);
    if (local) {
      const parsed = JSON.parse(local);
      exerciseCache.set(cacheKey, parsed);
      return parsed;
    }
  } catch { /* ignore parsing errors */ }

  try {
    const searchTerm = encodeURIComponent(name.toLowerCase().replace(/[^a-z0-9 ]/g, '').trim());

    // Default to the correct endpoint
    const url = `${EXERCISE_DB_BASE}/exercises/name/${searchTerm}?limit=5`;
    const options = {
      headers: {
        'x-rapidapi-key': RAPIDAPI_KEY,
        'x-rapidapi-host': 'exercisedb.p.rapidapi.com'
      }
    };

    const res = await fetch(url, RAPIDAPI_KEY ? options : undefined);
    if (!res.ok) return null;

    // The RapidAPI returns an array directly
    const json = await res.json();
    const exercises: ExerciseDbItem[] = Array.isArray(json) ? json : (json.data || []);

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
    try {
      localStorage.setItem(`gymbrain_exdb_${cacheKey}`, JSON.stringify(best));
    } catch { /* ignore quota errors */ }

    return best;
  } catch {
    return null;
  }
}