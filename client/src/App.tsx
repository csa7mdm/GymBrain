import { useEffect, useState } from 'react';
import { getProfile } from './services/api';
import { AuthProvider } from './context/AuthContext';
import { useAuth } from './context/auth';
import AuthPage from './pages/AuthPage';
import OnboardingPage from './pages/OnboardingPage';
import HomePage from './pages/HomePage';
import WorkoutPage from './pages/WorkoutPage';
import PlansPage from './pages/PlansPage';
import ProfilePage from './pages/ProfilePage';
import VaultPage from './pages/VaultPage';

type Tab = 'home' | 'train' | 'plans' | 'profile' | 'vault';

function BottomNav({ tab, setTab }: { tab: Tab; setTab: (t: Tab) => void }) {
  const items: { id: Tab; icon: string; label: string }[] = [
    { id: 'home', icon: '🏠', label: 'Home' },
    { id: 'train', icon: '💪', label: 'Train' },
    { id: 'plans', icon: '📋', label: 'History' },
    { id: 'profile', icon: '👤', label: 'Profile' },
    { id: 'vault', icon: '🔐', label: 'Vault' },
  ];
  return (
    <nav className="bottom-nav">
      {items.map(i => (
        <button key={i.id}
          className={`bottom-nav__item ${tab === i.id ? 'bottom-nav__item--active' : ''}`}
          onClick={() => setTab(i.id)}>
          <span className="bottom-nav__icon">{i.icon}</span>
          {i.label}
        </button>
      ))}
    </nav>
  );
}

function SignedInApp() {
  const [tab, setTab] = useState<Tab>('home');
  const { user } = useAuth();
  const [needsOnboarding, setNeedsOnboarding] = useState<boolean | null>(null);
  const [profileError, setProfileError] = useState('');
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    let cancelled = false;
    getProfile().then(result => {
      if (cancelled) return;
      if (!result.data) { setProfileError(result.error || 'Could not load your profile.'); return; }
      const profile = result.data;
      if (profile.goal) {
        let local = {};
        try { const value = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}'); if (value && typeof value === 'object' && !Array.isArray(value)) local = value; } catch { /* refresh invalid cache */ }
        let equipment: string[] = [];
        try { const value = JSON.parse(profile.equipmentJson || '[]'); if (Array.isArray(value)) equipment = value.filter(x => typeof x === 'string'); } catch { /* empty equipment */ }
        const old = local as { name?: string };
        localStorage.setItem('gymbrain_profile', JSON.stringify({ ...local,
          name: old.name || user?.email.split('@')[0] || 'Athlete',
          ...profile.personalProfile,
          goal: profile.goal, level: profile.experienceLevel, equipment, injuries: profile.injuries,
          daysPerWeek: profile.daysPerWeek, diet: profile.dietaryPreference, calories: profile.dailyCalories,
        }));
      }
      setNeedsOnboarding(!profile.goal);
      setProfileError('');
    });
    return () => { cancelled = true; };
  }, [attempt, user?.email]);

  if (profileError) return <div className="app-content"><p role="alert">{profileError}</p>
    <button className="m3-btn m3-btn--filled" onClick={() => { setProfileError(''); setAttempt(n => n + 1); }}>Retry profile</button></div>;
  if (needsOnboarding === null) return <div className="app-content" role="status">Loading your account…</div>;

  if (needsOnboarding) {
    return (
      <OnboardingPage onComplete={() => {
        setNeedsOnboarding(false);
        setTab('home');
      }} />
    );
  }

  return (
    <div className="app-shell">
      {tab === 'home' && <HomePage onNavigate={(t) => setTab(t as Tab)} />}
      {tab === 'train' && <WorkoutPage />}
      {tab === 'plans' && <PlansPage />}
      {tab === 'profile' && <ProfilePage />}
      {tab === 'vault' && <VaultPage onComplete={() => setTab('home')} onSkip={() => setTab('home')} />}
      <BottomNav tab={tab} setTab={setTab} />
    </div>
  );
}

function AppContent() {
  const { user } = useAuth();
  return user ? <SignedInApp key={user.userId} /> : <AuthPage />;
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}