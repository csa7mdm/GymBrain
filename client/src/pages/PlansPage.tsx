import { useState, useEffect } from 'react';

interface SavedWorkout {
  id: string; date: string; focus: string;
  exercises: { name: string; sets: number; reps: number; weight: number }[];
  completedSets: number; totalSets: number;
}

export default function PlansPage() {
  const [workouts, setWorkouts] = useState<SavedWorkout[]>([]);
  const [showCreatePlan, setShowCreatePlan] = useState(false);
  const [planName, setPlanName] = useState('');
  const [planDays, setPlanDays] = useState(4);

  useEffect(() => {
    const saved = JSON.parse(localStorage.getItem('gymbrain_workouts') || '[]');
    setWorkouts(saved);
  }, []);

  const deleteWorkout = (id: string) => {
    const updated = workouts.filter(w => w.id !== id);
    setWorkouts(updated);
    localStorage.setItem('gymbrain_workouts', JSON.stringify(updated));
  };

  const createPlan = () => {
    if (!planName.trim()) return;
    const plans = JSON.parse(localStorage.getItem('gymbrain_plans') || '[]');
    plans.push({ id: Date.now().toString(), name: planName, daysPerWeek: planDays, createdAt: new Date().toISOString() });
    localStorage.setItem('gymbrain_plans', JSON.stringify(plans));
    setPlanName('');
    setShowCreatePlan(false);
  };

  const plans = JSON.parse(localStorage.getItem('gymbrain_plans') || '[]');

  return (
    <div className="app-content fade-in">
      <h2 className="md-headline-sm mb-md">📋 Plans</h2>

      {/* Create Plan CTA */}
      <button className="m3-btn m3-btn--tonal m3-btn--full mb-lg" onClick={() => setShowCreatePlan(!showCreatePlan)}>
        {showCreatePlan ? '✕ Cancel' : '+ Create Plan'}
      </button>

      {showCreatePlan && (
        <div className="m3-card mb-lg slide-up">
          <div className="md-title-md mb-md">New Training Plan</div>
          <div className="m3-field">
            <label className="m3-field__label" htmlFor="plan-name">Plan Name</label>
            <input id="plan-name" className="m3-input" placeholder="e.g., Push Pull Legs"
              value={planName} onChange={e => setPlanName(e.target.value)} autoFocus />
          </div>
          <div className="md-label-md text-muted mb-sm">Days per Week</div>
          <div className="m3-segmented mb-md">
            {[2, 3, 4, 5, 6].map(d => (
              <button key={d} className={`m3-segmented__btn ${planDays === d ? 'm3-segmented__btn--active' : ''}`}
                onClick={() => setPlanDays(d)}>
                {d}x
              </button>
            ))}
          </div>
          <button className="m3-btn m3-btn--filled m3-btn--full" disabled={!planName.trim()} onClick={createPlan}>
            Create Plan
          </button>
        </div>
      )}

      {/* Active Plans & Cycle Progression */}
      {plans.length > 0 && (
        <>
          <div className="section-header">
            <span className="section-header__icon">🎯</span>
            <span className="section-header__title">Training Journey</span>
          </div>
          {plans.map((plan: any) => {
            const cycleLength = plan.daysPerWeek * 4; // Assume 4-week cycles
            const completedInCycle = workouts.length % cycleLength;
            const progressPct = Math.round((completedInCycle / cycleLength) * 100);

            return (
              <div key={plan.id} className="m3-card mb-md">
                <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 12 }}>
                  <div style={{ flex: 1 }}>
                    <div className="md-title-md">{plan.name}</div>
                    <div className="md-body-sm text-muted">{plan.daysPerWeek}x per week</div>
                  </div>
                  <span className="chip chip-success">Active</span>
                </div>

                {/* Cycle Progression */}
                <div className="md-label-md text-muted mb-sm">Cycle Progression</div>
                <div style={{ height: 8, background: 'rgba(255,255,255,0.1)', borderRadius: 4, overflow: 'hidden', marginBottom: 8 }}>
                  <div style={{ height: '100%', width: `${progressPct}%`, background: 'var(--md-primary)' }} />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--md-on-surface-variant)' }}>
                  <span>{completedInCycle} / {cycleLength} Sessions</span>
                  <span>Week {Math.floor(completedInCycle / plan.daysPerWeek) + 1} of 4</span>
                </div>
              </div>
            );
          })}
        </>
      )}

      {/* Saved Workouts */}
      <div className="section-header">
        <span className="section-header__icon">💾</span>
        <span className="section-header__title">Saved Workouts ({workouts.length})</span>
      </div>

      {workouts.length === 0 ? (
        <div className="plans-empty">
          <div className="plans-empty__icon">📂</div>
          <div className="plans-empty__text">No saved workouts yet. Complete a training session and save it!</div>
        </div>
      ) : (
        workouts.map(w => (
          <div key={w.id} className="saved-workout">
            <div className="saved-workout__icon">💪</div>
            <div className="saved-workout__info">
              <div className="saved-workout__name">{w.focus}</div>
              <div className="saved-workout__meta">
                {w.exercises?.length || 0} exercises · {w.completedSets}/{w.totalSets} sets · {new Date(w.date).toLocaleDateString()}
              </div>
            </div>
            <div className="saved-workout__actions">
              <button title="Delete" onClick={() => deleteWorkout(w.id)}>🗑️</button>
            </div>
          </div>
        ))
      )}
    </div>
  );
}