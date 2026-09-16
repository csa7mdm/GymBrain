import { API_BASE } from './api';
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

function isExercise(value: unknown): value is ExerciseDbItem {
  if (!value || typeof value !== 'object') return false;
  const item = value as Record<string, unknown>;
  return ['id', 'name', 'target', 'bodyPart', 'equipment'].every(key => typeof item[key] === 'string')
    && typeof item.gifUrl === 'string'
    && (item.gifUrl === '' || /^https?:\/\//i.test(item.gifUrl))
    && Array.isArray(item.secondaryMuscles) && item.secondaryMuscles.every(x => typeof x === 'string')
    && Array.isArray(item.instructions) && item.instructions.every(x => typeof x === 'string');
}

const exerciseCache = new Map<string, ExerciseDbItem>();

export async function searchExercise(name: string): Promise<ExerciseDbItem | null> {
  const cacheKey = name.toLowerCase().trim();

  if (exerciseCache.has(cacheKey)) return exerciseCache.get(cacheKey)!;

  try {
    const local = localStorage.getItem(`gymbrain_exdb_${cacheKey}`);
    if (local) {
      const parsed = JSON.parse(local);
      if (isExercise(parsed)) {
        exerciseCache.set(cacheKey, parsed);
        return parsed;
      }
      localStorage.removeItem(`gymbrain_exdb_${cacheKey}`);
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
    if (!isExercise(raw)) return null;

    const best = raw;
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
