import { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import AuthPage from './pages/AuthPage';
import VaultPage from './pages/VaultPage';
import HomePage from './pages/HomePage';
import WorkoutPage from './pages/WorkoutPage';
import PlansPage from './pages/PlansPage';
import ProfilePage from './pages/ProfilePage';
import './index.css';

type Screen = 'auth' | 'vault' | 'home' | 'train' | 'plans' | 'profile';

function BottomNav({ active, onChange }: { active: string; onChange: (t: string) => void }) {
  const tabs = [
    { id: 'home', icon: '🏠', label: 'Home' },
    { id: 'train', icon: '💪', label: 'Train' },
    { id: 'plans', icon: '📋', label: 'Plans' },
    { id: 'profile', icon: '👤', label: 'Profile' },
  ];
  return (
    <nav className="bottom-nav">
      {tabs.map(t => (
        <button key={t.id}
          className={`bottom-nav__item ${active === t.id ? 'bottom-nav__item--active' : ''}`}
          onClick={() => onChange(t.id)}>
          <span className="bottom-nav__icon">{t.icon}</span>
          <span>{t.label}</span>
        </button>
      ))}
    </nav>
  );
}

function AppContent() {
  const { isAuthenticated } = useAuth();
  const [screen, setScreen] = useState<Screen>('auth');

  if (!isAuthenticated) {
    return <AuthPage onSuccess={() => setScreen('vault')} />;
  }

  if (screen === 'vault' || screen === 'auth') {
    return (
      <VaultPage
        onComplete={() => setScreen('home')}
        onSkip={() => setScreen('home')}
      />
    );
  }

  const activeTab = screen;
  const navigate = (tab: string) => setScreen(tab as Screen);

  return (
    <div className="app-shell">
      {activeTab === 'home' && <HomePage onNavigate={navigate} />}
      {activeTab === 'train' && <WorkoutPage />}
      {activeTab === 'plans' && <PlansPage onNavigate={navigate} />}
      {activeTab === 'profile' && <ProfilePage />}
      <BottomNav active={activeTab} onChange={navigate} />
    </div>
  );
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;