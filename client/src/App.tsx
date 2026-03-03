import { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import AuthPage from './pages/AuthPage';
import VaultPage from './pages/VaultPage';
import WorkoutPage from './pages/WorkoutPage';
import './index.css';

type Screen = 'auth' | 'vault' | 'workout';

function AppContent() {
  const { isAuthenticated } = useAuth();
  const [screen, setScreen] = useState<Screen>('auth');

  if (!isAuthenticated) {
    return <AuthPage onSuccess={() => setScreen('vault')} />;
  }

  switch (screen) {
    case 'vault':
      return (
        <VaultPage
          onComplete={() => setScreen('workout')}
          onSkip={() => setScreen('workout')}
        />
      );
    case 'workout':
      return <WorkoutPage />;
    default:
      return <WorkoutPage />;
  }
}

function App() {
  return (
    <AuthProvider>
      <div className="app-container">
        <AppContent />
      </div>
    </AuthProvider>
  );
}

export default App;
