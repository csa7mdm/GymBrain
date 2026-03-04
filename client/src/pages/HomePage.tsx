import { useAuth } from '../context/AuthContext';

const QUOTES = [
  { text: "The only bad workout is the one that didn't happen.", author: "Unknown" },
  { text: "Take care of your body. It's the only place you have to live.", author: "Jim Rohn" },
  { text: "Strength does not come from the body. It comes from the will.", author: "Gandhi" },
  { text: "Your body can stand almost anything. It's your mind you have to convince.", author: "Unknown" },
  { text: "The pain you feel today will be the strength you feel tomorrow.", author: "Arnold" },
];

export default function HomePage({ onNavigate }: { onNavigate: (tab: string) => void }) {
  const { user } = useAuth();
  const saved = JSON.parse(localStorage.getItem('gymbrain_workouts') || '[]');
  const profile = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
  const q = QUOTES[Math.floor(Math.random() * QUOTES.length)];
  const today = new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });

  return (
    <div className="app-content">
      <div className="home-hero">
        <p className="home-hero__greeting">Hello, <span>{profile.name || user?.email?.split('@')[0] || 'Athlete'}</span> 👋</p>
        <p className="home-hero__date">{today}</p>
      </div>

      <div className="home-stats">
        <div className="home-stat">
          <span className="home-stat__val">{saved.length}</span>
          <span className="home-stat__label">Workouts</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__val">{saved.reduce((s: number, w: any) => s + (w.exercises?.length || 0), 0)}</span>
          <span className="home-stat__label">Exercises</span>
        </div>
        <div className="home-stat">
          <span className="home-stat__val">{profile.weight || '—'}</span>
          <span className="home-stat__label">kg</span>
        </div>
      </div>

      <div className="home-quote">
        <p className="home-quote__text">"{q.text}"</p>
        <p className="home-quote__author">— {q.author}</p>
      </div>

      <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" onClick={() => onNavigate('train')}>
        💪 Start Training
      </button>

      {saved.length === 0 && (
        <div className="m3-card m3-card--outlined" style={{ marginTop: 16, textAlign: 'center' }}>
          <p className="md-body-md text-muted">No saved workouts yet</p>
          <p className="md-body-sm text-muted" style={{ marginTop: 4 }}>Generate your first AI workout to get started!</p>
        </div>
      )}

      {saved.length > 0 && (
        <div style={{ marginTop: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span className="md-title-sm">Recent Workouts</span>
            <button className="m3-btn m3-btn--text" style={{ padding: '4px 8px', minHeight: 'auto', fontSize: '0.75rem' }} onClick={() => onNavigate('plans')}>See All →</button>
          </div>
          {saved.slice(-2).reverse().map((w: any, i: number) => (
            <div key={i} className="plan-item">
              <div className="plan-item__icon">💪</div>
              <div className="plan-item__body">
                <div className="plan-item__name">{w.focus || 'Full Body'} Workout</div>
                <div className="plan-item__meta">{w.exercises?.length || 0} exercises • {new Date(w.date).toLocaleDateString()}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      {!profile.name && (
        <div className="m3-card m3-card--filled" style={{ marginTop: 16, textAlign: 'center' }}>
          <p className="md-body-md" style={{ marginBottom: 8 }}>📊 Set up your profile for personalized workouts</p>
          <button className="m3-btn m3-btn--tonal" onClick={() => onNavigate('profile')}>Complete Profile →</button>
        </div>
      )}
    </div>
  );
}