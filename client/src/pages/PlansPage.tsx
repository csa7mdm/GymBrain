import { readCompletion, useWorkoutHistory } from '../services/workoutHistory';

export default function PlansPage() {
  const { history, error, retry, loadMore, loadingMore } = useWorkoutHistory();
  return <div className="app-content fade-in">
    <h2 className="md-headline-sm mb-md">Workout History</h2>
    <p className="md-body-sm text-muted">Completed sessions saved to your account.</p>
    {error && <div role="alert" className="m3-error-banner">Could not load history: {error}
      <button className="m3-btn m3-btn--outlined" onClick={retry}>Retry history</button>
    </div>}
    {!history && !error && <p role="status">Loading history…</p>}
    {history && <p>{history.total} saved sessions</p>}
    {history?.total === 0 && <p>No saved workouts yet. Complete a training session and save it.</p>}
    {history?.items.map(item => {
      const completion = readCompletion(item);
      const sets = completion?.exercises.flatMap(ex => ex.sets) || [];
      return <details key={item.id} className="m3-card mb-md">
        <summary>
          <strong>{completion?.focus || 'Workout'}</strong> · {new Date(item.completedAtUtc).toLocaleDateString()}
          {completion && <span> · {sets.filter(set => set.completed).length}/{sets.length} sets</span>}
        </summary>
        {completion ? completion.exercises.map((ex, index) => <div key={index} className="mt-md">
          <h3 className="md-title-md">{ex.name}</h3>
          {ex.sets.map((set, i) => <p key={i}>Set {i + 1}: {set.completed ? `${set.reps} reps × ${set.weightKg} kg` : 'Not completed'}</p>)}
        </div>) : <p>Saved with an earlier version. Actual set results were not recorded.</p>}
      </details>;
    })}
    {history?.hasMore && <button className="m3-btn m3-btn--outlined" disabled={loadingMore} onClick={loadMore}>
      {loadingMore ? 'Loading…' : 'Load more'}
    </button>}
  </div>;
}
