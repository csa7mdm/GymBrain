import { useState } from 'react';
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
    { id: 'plans', icon: '📋', label: 'Plans' },
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
  const [needsOnboarding, setNeedsOnboarding] = useState(() => {
    const profile = localStorage.getItem('gymbrain_profile');
    try {
      return !profile || !JSON.parse(profile).name;
    } catch {
      return true;
    }
  });

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