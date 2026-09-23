import { useEffect, useState } from 'react';
import { readCompletion, useWorkoutHistory } from '../services/workoutHistory';
import { getLatestMealPlan } from '../services/api';
import { parseMealPlan } from '../services/mealPlan';
import './HomePage.css';

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

interface HomePageProps {
  onNavigate: (tab: string) => void;
}

export default function HomePage({ onNavigate }: HomePageProps) {
  let profile: { name?: string; goal?: string; level?: string; daysPerWeek?: number } = {};
  try {
    const cached = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
    if (cached && typeof cached === 'object' && !Array.isArray(cached)) profile = cached;
  } catch { /* server-backed pages remain available without the display cache */ }
  const [mealSummary, setMealSummary] = useState<{ date: string; count: number } | null>(null);
  const [mealLoading, setMealLoading] = useState(true);
  const [mealError, setMealError] = useState(false);
  useEffect(() => {
    let cancelled = false;
    getLatestMealPlan().then(result => {
      if (cancelled) return;
      if (result.error) setMealError(true);
      else if (result.data) {
        try {
          setMealSummary({ date: result.data.generatedAtUtc, count: parseMealPlan(result.data.payloadJson).meals.length });
        } catch { setMealError(true); }
      }
      setMealLoading(false);
    });
    return () => { cancelled = true; };
  }, []);
  const { history, error: historyError, retry } = useWorkoutHistory();
  const workouts = (history?.items || []).map(item => {
    const completion = readCompletion(item);
    const sets = completion?.exercises.flatMap(ex => ex.sets) || [];
    return { hasResults: completion !== null, id: item.id, date: item.completedAtUtc, focus: completion?.focus || 'Workout',
      exercises: completion?.exercises || [], completedSets: sets.filter(set => set.completed).length, totalSets: sets.length };
  });
  const recordedWorkouts = workouts.filter(workout => workout.hasResults);
  const missingResults = workouts.length - recordedWorkouts.length;
  const name = typeof profile.name === 'string' ? profile.name : 'Athlete';

  // Calculate stats
  const totalWorkouts = history?.total ?? '—';
  const totalExercises = workouts.reduce((s, w) => s + (w.exercises?.length || 0), 0);

  // Weekly view
  const today = new Date();
  const dayOfWeek = today.getDay();
  const workoutDates = workouts.map(w => new Date(w.date).toDateString());
  const weekDays = DAY_NAMES.map((name, i) => {
    const date = new Date(today);
    date.setDate(today.getDate() - dayOfWeek + i);
    const isDone = workoutDates.includes(date.toDateString());
    const isToday = i === dayOfWeek;
    return { name, isDone, isToday };
  });
  const thisWeekCount = weekDays.filter(day => day.isDone).length;

  // Streak (consecutive days with workouts ending today)
  let streak = 0;
  const streakDate = new Date();
  while (workoutDates.includes(streakDate.toDateString())) {
    streak++;
    streakDate.setDate(streakDate.getDate() - 1);
  }

  const goalLabels: Record<string, string> = {
    'muscle': '💪 Build Muscle', 'fat-loss': '🔥 Lose Fat', 'strength': '🏋️ Get Strong',
    'endurance': '🏃 Endurance', 'health': '❤️ Stay Healthy',
  };

  const hasProfile = !!profile.name;

  return (
    <div className="app-content fade-in">
      {/* Greeting */}
      <div className="home-greeting">
        <div className="home-greeting__hello">Hello,</div>
        <div className="home-greeting__name">{name} 👋</div>
        {streak > 0 && (
          <div className="home-greeting__streak">🔥 {streak} day streak</div>
        )}
      </div>

      {historyError && <div role="alert" className="m3-error-banner">History is unavailable.
        <button className="m3-btn m3-btn--outlined" onClick={retry}>Retry history</button></div>}
      {/* Stats */}
      <div className="home-stats">
        <div className="home-stat">
          <span className="home-stat__value">{totalWorkouts}</span>
          <span className="home-stat__label">Workouts</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__value">{missingResults ? '—' : totalExercises}</span>
          <span className="home-stat__label">Recent exercises</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__value">{thisWeekCount}</span>
          <span className="home-stat__label">This week</span>
        </div>
      </div>

      <section className="m3-card home-meal mb-md" aria-labelledby="home-meal-title">
        <div className="home-meal__header"><span aria-hidden="true">🍽️</span><h2 id="home-meal-title">Your meal plan</h2></div>
        {mealLoading ? <p role="status" className="md-body-md text-muted">Checking saved plan…</p>
          : mealError ? <p role="alert" className="md-body-md">Saved plan is temporarily unavailable.</p>
          : mealSummary ? <p className="md-body-md text-muted">{mealSummary.count} meals saved · {new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(mealSummary.date))}</p>
          : <p className="md-body-md text-muted">Create a plan once, then find it here whenever you return.</p>}
        <button type="button" className="m3-btn m3-btn--outlined" onClick={() => onNavigate('meals')}>
          {mealSummary ? 'View saved meals' : 'Open meals'}
        </button>
      </section>

      {/* CTA - moved to top for dominance */}
      <div className="mt-md mb-md">
        <button className="m3-fab m3-fab--full" style={{ minHeight: 64, fontSize: '1.2rem', fontWeight: 800, background: 'var(--md-primary)', color: '#000' }} onClick={() => onNavigate('train')}>
          <span className="m3-fab__icon">💪</span>
          Start Training
        </button>
      </div>

      {/* Weekly Calendar */}
      <div className="home-week">
        {weekDays.map((d, i) => (
          <div key={i} className={`home-week__day ${d.isDone ? 'home-week__day--done' : ''} ${d.isToday ? 'home-week__day--today' : ''}`}>
            <span>{d.name}</span>
            <span>{d.isDone ? '✓' : '·'}</span>
          </div>
        ))}
      </div>

      {/* Goal + Level */}
      {hasProfile && (
        <div className="m3-card mb-md" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <div className="md-label-md text-muted">Goal</div>
            <div className="md-body-lg">{profile.goal ? goalLabels[profile.goal] || profile.goal : 'Choose a goal'}</div>
          </div>
          <div style={{ textAlign: 'right' }}>
            <div className="md-label-md text-muted">Level</div>
            <div className="md-body-lg">{profile.level}</div>
          </div>
          <div style={{ textAlign: 'right' }}>
            <div className="md-label-md text-muted">Plan</div>
            <div className="md-body-lg">{profile.daysPerWeek}x/wk</div>
          </div>
        </div>
      )}

      {/* Summaries only include sessions with recorded results. */}
      {recordedWorkouts.length > 0 && (
        <>
          <div className="section-header mt-lg" style={{ marginBottom: 16 }}>
            <span className="section-header__icon">🧬</span>
            <span className="section-header__title">Recent Recorded Results</span>
          </div>
          <div className="m3-card mb-lg">
            <div className="md-body-sm text-muted mb-sm">Consistency (Streak)</div>
            <div style={{ height: 8, background: 'rgba(255,255,255,0.1)', borderRadius: 4, overflow: 'hidden', marginBottom: 16 }}>
              <div style={{ height: '100%', width: `${Math.min(streak * 10, 100)}%`, background: 'var(--md-primary)' }} />
            </div>

            <div className="md-body-sm text-muted mb-sm">Recent Completion Rate</div>
            <div style={{ height: 8, background: 'rgba(255,255,255,0.1)', borderRadius: 4, overflow: 'hidden', marginBottom: 16 }}>
              <div style={{ height: '100%', width: `${totalWorkouts ? Math.round((recordedWorkouts.reduce((s, w) => s + w.completedSets, 0) / Math.max(recordedWorkouts.reduce((s, w) => s + w.totalSets, 0), 1)) * 100) : 0}%`, background: 'var(--md-tertiary)' }} />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--md-on-surface-variant)' }}>
              <span>Recent Sets Completed: {recordedWorkouts.reduce((s, w) => s + w.completedSets, 0)}</span>
              <span>Avg Sets/Workout: {totalWorkouts ? Math.round(recordedWorkouts.reduce((s, w) => s + w.completedSets, 0) / recordedWorkouts.length) : 0}</span>
            </div>
          </div>
        </>
      )}

      {missingResults > 0 && <p className="md-body-sm text-muted">Some older workouts have no recorded set results. They count toward your workout total but are excluded from set statistics.</p>}

      {/* Recent Workouts */}
      {workouts.length > 0 && (
        <>
          <div className="section-header">
            <span className="section-header__icon">📋</span>
            <span className="section-header__title">Recent Workouts</span>
          </div>
          {workouts.slice(0, 2).map(w => (
            <div key={w.id} className="saved-workout">
              <div className="saved-workout__icon">💪</div>
              <div className="saved-workout__info">
                <div className="saved-workout__name">{w.focus}</div>
                <div className="saved-workout__meta">
                  {w.hasResults
                    ? `${w.exercises.length} exercises · ${w.completedSets}/${w.totalSets} sets`
                    : 'Older workout · set results unavailable'}
                </div>
              </div>
            </div>
          ))}
        </>
      )}

      {/* CTA moved to top */}

      {/* Profile CTA (if incomplete) */}
      {!hasProfile && (
        <div className="m3-card--outlined m3-card mt-md" style={{ textAlign: 'center' }}>
          <p className="md-body-md text-muted mb-sm">Complete your profile for personalized workouts</p>
          <button className="m3-btn m3-btn--tonal m3-btn--sm" onClick={() => onNavigate('profile')}>
            Setup Profile →
          </button>
        </div>
      )}
    </div>
  );
}
