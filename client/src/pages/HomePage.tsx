import { useState, useEffect } from 'react';

const QUOTES = [
  { text: "The body achieves what the mind believes.", author: "Napoleon Hill" },
  { text: "The only bad workout is the one that didn't happen.", author: "Unknown" },
  { text: "Strength does not come from the body. It comes from the will.", author: "Gandhi" },
  { text: "Push yourself, because no one else is going to do it for you.", author: "Unknown" },
  { text: "Your body can stand almost anything. It's your mind you have to convince.", author: "Unknown" },
  { text: "Success isn't always about greatness. It's about consistency.", author: "Dwayne Johnson" },
  { text: "Don't count the days, make the days count.", author: "Muhammad Ali" },
];

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

interface HomePageProps {
  onNavigate: (tab: string) => void;
}

interface SavedWorkout {
  id: string; date: string; focus: string;
  exercises: { name: string; sets: number; reps: number; weight: number }[];
  completedSets: number; totalSets: number;
}

export default function HomePage({ onNavigate }: HomePageProps) {
  const [quote] = useState(() => QUOTES[Math.floor(Math.random() * QUOTES.length)]);

  const profile = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
  const workouts: SavedWorkout[] = JSON.parse(localStorage.getItem('gymbrain_workouts') || '[]');
  const name = profile.name || 'Athlete';

  // Calculate stats
  const totalWorkouts = workouts.length;
  const totalExercises = workouts.reduce((s, w) => s + (w.exercises?.length || 0), 0);
  const bmi = profile.height && profile.weight
    ? (profile.weight / ((profile.height / 100) ** 2)).toFixed(1)
    : null;

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

  // Streak (consecutive days with workouts ending today)
  const [streak, setStreak] = useState(0);
  useEffect(() => {
    let s = 0;
    const d = new Date();
    while (true) {
      if (workoutDates.includes(d.toDateString())) { s++; d.setDate(d.getDate() - 1); }
      else break;
    }
    setStreak(s);
  }, []);

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

      {/* Stats */}
      <div className="home-stats">
        <div className="home-stat">
          <span className="home-stat__value">{totalWorkouts}</span>
          <span className="home-stat__label">Workouts</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__value">{totalExercises}</span>
          <span className="home-stat__label">Exercises</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__value">{bmi || '—'}</span>
          <span className="home-stat__label">BMI</span>
        </div>
      </div>

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
            <div className="md-body-lg">{goalLabels[profile.goal] || profile.goal}</div>
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

      {/* Quote */}
      <div className="home-quote">
        <div className="home-quote__text">"{quote.text}"</div>
        <div className="home-quote__author">— {quote.author}</div>
      </div>

      {/* AI Analytics: Health Pillars */}
      {workouts.length > 0 && (
        <>
          <div className="section-header mt-lg" style={{ marginBottom: 16 }}>
            <span className="section-header__icon">🧬</span>
            <span className="section-header__title">Health Pillars</span>
          </div>
          <div className="m3-card mb-lg">
            <div className="md-body-sm text-muted mb-sm">Consistency (Streak)</div>
            <div style={{ height: 8, background: 'rgba(255,255,255,0.1)', borderRadius: 4, overflow: 'hidden', marginBottom: 16 }}>
              <div style={{ height: '100%', width: `${Math.min(streak * 10, 100)}%`, background: 'var(--md-primary)' }} />
            </div>

            <div className="md-body-sm text-muted mb-sm">Overall Completion Rate</div>
            <div style={{ height: 8, background: 'rgba(255,255,255,0.1)', borderRadius: 4, overflow: 'hidden', marginBottom: 16 }}>
              <div style={{ height: '100%', width: `${totalWorkouts ? Math.round((workouts.reduce((s, w) => s + w.completedSets, 0) / Math.max(workouts.reduce((s, w) => s + w.totalSets, 0), 1)) * 100) : 0}%`, background: 'var(--md-tertiary)' }} />
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--md-on-surface-variant)' }}>
              <span>Total Sets Completed: {workouts.reduce((s, w) => s + w.completedSets, 0)}</span>
              <span>Avg Sets/Workout: {totalWorkouts ? Math.round(workouts.reduce((s, w) => s + w.completedSets, 0) / totalWorkouts) : 0}</span>
            </div>
          </div>
        </>
      )}

      {/* Recent Workouts */}
      {workouts.length > 0 && (
        <>
          <div className="section-header">
            <span className="section-header__icon">📋</span>
            <span className="section-header__title">Recent Workouts</span>
          </div>
          {workouts.slice(-2).reverse().map(w => (
            <div key={w.id} className="saved-workout">
              <div className="saved-workout__icon">💪</div>
              <div className="saved-workout__info">
                <div className="saved-workout__name">{w.focus}</div>
                <div className="saved-workout__meta">
                  {w.exercises?.length || 0} exercises · {w.completedSets}/{w.totalSets} sets
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