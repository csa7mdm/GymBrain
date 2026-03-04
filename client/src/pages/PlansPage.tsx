import { useState, useEffect } from 'react';

interface SavedWorkout {
  id: string;
  date: string;
  focus: string;
  exercises: { name: string; sets: number; reps: number; weight: number }[];
  completedSets?: number;
  totalSets?: number;
}

export default function PlansPage({ onNavigate }: { onNavigate: (tab: string) => void }) {
  const [workouts, setWorkouts] = useState<SavedWorkout[]>([]);
  const [showCreate, setShowCreate] = useState(false);
  const [planName, setPlanName] = useState('');
  const [planDays, setPlanDays] = useState(4);

  useEffect(() => {
    const stored = JSON.parse(localStorage.getItem('gymbrain_workouts') || '[]');
    setWorkouts(stored);
  }, []);

  const deleteWorkout = (id: string) => {
    const updated = workouts.filter(w => w.id !== id);
    setWorkouts(updated);
    localStorage.setItem('gymbrain_workouts', JSON.stringify(updated));
  };

  const createPlan = () => {
    if (!planName.trim()) return;
    const plan = {
      id: Date.now().toString(),
      name: planName,
      days: planDays,
      createdAt: new Date().toISOString(),
      workouts: [],
    };
    const plans = JSON.parse(localStorage.getItem('gymbrain_plans') || '[]');
    plans.push(plan);
    localStorage.setItem('gymbrain_plans', JSON.stringify(plans));
    setPlanName('');
    setShowCreate(false);
  };

  const plans = JSON.parse(localStorage.getItem('gymbrain_plans') || '[]');

  return (
    <div className="app-content">
      <div style={{ textAlign: 'center', marginBottom: 20 }}>
        <div style={{ fontSize: '2.5rem', marginBottom: 4 }}>📋</div>
        <h2 className="md-headline-sm" style={{ color: 'var(--md-primary)' }}>Training Plans</h2>
        <p className="md-body-sm text-muted">Saved workouts & custom cycles</p>
      </div>

      {/* Create Plan Section */}
      <div className="profile-section">
        <h3 className="profile-section__title">🆕 Create Custom Plan</h3>
        {!showCreate ? (
          <button className="m3-btn m3-btn--tonal m3-btn--full" onClick={() => setShowCreate(true)}>
            ➕ New Training Cycle
          </button>
        ) : (
          <div className="m3-card m3-card--outlined">
            <div className="m3-field">
              <label className="m3-field__label">Plan Name</label>
              <input className="m3-input" placeholder="e.g. Push/Pull/Legs" value={planName} onChange={e => setPlanName(e.target.value)} />
            </div>
            <div className="m3-field">
              <label className="m3-field__label">Days per Week</label>
              <div style={{ display: 'flex', gap: 8 }}>
                {[2,3,4,5,6].map(d => (
                  <button key={d} className={`m3-chip ${planDays === d ? 'm3-chip--selected' : ''}`}
                    onClick={() => setPlanDays(d)}>{d}</button>
                ))}
              </div>
            </div>
            <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
              <button className="m3-btn m3-btn--filled" style={{ flex: 1 }} onClick={createPlan}>Create</button>
              <button className="m3-btn m3-btn--outlined" onClick={() => setShowCreate(false)}>Cancel</button>
            </div>
          </div>
        )}
      </div>

      {/* Active Plans */}
      {plans.length > 0 && (
        <div className="profile-section">
          <h3 className="profile-section__title">📅 Active Plans</h3>
          {plans.map((p: any) => (
            <div key={p.id} className="plan-item">
              <div className="plan-item__icon">📅</div>
              <div className="plan-item__body">
                <div className="plan-item__name">{p.name}</div>
                <div className="plan-item__meta">{p.days} days/week • Created {new Date(p.createdAt).toLocaleDateString()}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="m3-divider" />

      {/* Saved Workouts */}
      <div className="profile-section">
        <h3 className="profile-section__title">💾 Saved Workouts ({workouts.length})</h3>
        {workouts.length === 0 ? (
          <div className="m3-card m3-card--filled" style={{ textAlign: 'center' }}>
            <p className="md-body-md text-muted">No saved workouts yet</p>
            <p className="md-body-sm text-muted" style={{ marginTop: 4 }}>Complete a workout on the Train tab to save it here</p>
            <button className="m3-btn m3-btn--tonal" style={{ marginTop: 12 }} onClick={() => onNavigate('train')}>
              Go to Training →
            </button>
          </div>
        ) : (
          workouts.slice().reverse().map(w => (
            <div key={w.id} className="plan-item" style={{ position: 'relative' }}>
              <div className="plan-item__icon">💪</div>
              <div className="plan-item__body">
                <div className="plan-item__name">{w.focus || 'Full Body'}</div>
                <div className="plan-item__meta">
                  {w.exercises?.length || 0} exercises • {w.totalSets ? `${w.completedSets}/${w.totalSets} sets` : ''} • {new Date(w.date).toLocaleDateString()}
                </div>
              </div>
              <button className="m3-btn m3-btn--text" style={{ padding: '4px 8px', minHeight: 'auto', fontSize: '0.7rem', color: 'var(--md-error)' }}
                onClick={() => deleteWorkout(w.id)}>✕</button>
            </div>
          ))
        )}
      </div>

      <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" style={{ marginTop: 8 }} onClick={() => onNavigate('train')}>
        💪 Generate New Workout
      </button>
    </div>
  );
}