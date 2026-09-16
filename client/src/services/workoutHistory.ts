import { useEffect, useState } from 'react';
import { getWorkoutHistory, type WorkoutHistoryItem, type WorkoutHistoryResponse } from './api';

export interface RecordedSet { completed: boolean; reps: number; weightKg: number; }
export interface RecordedExercise { exerciseId?: string; name: string; sets: RecordedSet[]; }
export interface Completion { schemaVersion: 1; focus: string; exercises: RecordedExercise[]; }
export function readCompletion(item: WorkoutHistoryItem): Completion | null {
  try {
    const data = JSON.parse(item.payloadJson);
    if (data.schemaVersion !== 1 || typeof data.focus !== 'string' || !Array.isArray(data.exercises)) return null;
    for (const ex of data.exercises) {
      if (typeof ex?.name !== 'string' || !Array.isArray(ex.sets)) return null;
      for (const set of ex.sets) {
        if (typeof set?.completed !== 'boolean' || !Number.isFinite(set.reps) || !Number.isFinite(set.weightKg)) return null;
      }
    }
    return data as Completion;
  } catch { return null; }
}

export function useWorkoutHistory() {
  const [history, setHistory] = useState<WorkoutHistoryResponse | null>(null);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [loadingMore, setLoadingMore] = useState(false);
  useEffect(() => {
    let cancelled = false;
    getWorkoutHistory().then(result => {
      if (cancelled) return;
      setError(result.error || '');
      if (result.data) setHistory(result.data);
    });
    return () => { cancelled = true; };
  }, [attempt]);
  const loadMore = async () => {
    if (!history || loadingMore) return;
    setLoadingMore(true);
    const result = await getWorkoutHistory(history.items.length);
    setLoadingMore(false);
    setError(result.error || '');
    if (result.data) {
      const next = result.data;
      setHistory({ ...next, items: [...history.items, ...next.items.filter(item => !history.items.some(old => old.id === item.id))] });
    }
  };
  return { history, error, retry: () => { setError(''); setAttempt(n => n + 1); }, loadMore, loadingMore };
}
