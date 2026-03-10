import { API_BASE } from './api';
import { ExerciseSchema, type ValidatedExercise } from '../types/schemas';

export type ExerciseDbItem = ValidatedExercise;

const exerciseCache = new Map<string, ExerciseDbItem>();

export async function searchExercise(name: string): Promise<ExerciseDbItem | null> {
  const cacheKey = name.toLowerCase().trim();

  if (exerciseCache.has(cacheKey)) return exerciseCache.get(cacheKey)!;

  try {
    const local = localStorage.getItem(`gymbrain_exdb_${cacheKey}`);
    if (local) {
      const parsed = JSON.parse(local);
      exerciseCache.set(cacheKey, parsed);
      return parsed;
    }
  } catch {
    // Ignore parsing errors and refetch.
  }

  try {
    const token = localStorage.getItem('gymbrain_token');
    const response = await fetch(`${API_BASE}/api/workout/exercise-metadata/${encodeURIComponent(name)}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {}
    });

    if (response.status === 204 || !response.ok) return null;

    const raw = await response.json();
    const result = ExerciseSchema.safeParse(raw);
    if (!result.success) return null;

    const best = result.data;
    exerciseCache.set(cacheKey, best);

    try {
      localStorage.setItem(`gymbrain_exdb_${cacheKey}`, JSON.stringify(best));
    } catch {
      // Ignore quota errors.
    }

    return best;
  } catch {
    return null;
  }
}
