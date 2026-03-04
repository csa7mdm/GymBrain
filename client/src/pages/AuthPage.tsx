import { useState } from 'react';
import { login, register } from '../services/api';
import { useAuth } from '../context/AuthContext';

export default function AuthPage() {
    const { setAuth } = useAuth();
    const [isLogin, setIsLogin] = useState(true);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [tonePersona, setTonePersona] = useState('Motivational Coach');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        const result = isLogin
            ? await login(email, password)
            : await register(email, password, tonePersona);

        setLoading(false);

        if (result.error) { setError(result.error); return; }
        if (result.data) {
            setAuth(result.data.userId, email, result.data.token);
        }
    };

    return (
        <div className="auth-page">
            <div className="auth-page__logo">🧠</div>
            <h1 className="auth-page__title">GymBrain</h1>
            <p className="auth-page__subtitle">AI-Powered Fitness Coaching</p>

            <form className="auth-form" onSubmit={handleSubmit}>
                <h2 className="md-title-lg text-center mb-md">
                    {isLogin ? 'Welcome Back' : 'Create Account'}
                </h2>

                <div className="m3-field">
                    <label className="m3-field__label" htmlFor="email">Email</label>
                    <input id="email" className="m3-input" type="email" placeholder="you@example.com"
                        value={email} onChange={e => setEmail(e.target.value)}
                        required autoComplete="email" />
                </div>

                <div className="m3-field">
                    <label className="m3-field__label" htmlFor="password">Password</label>
                    <input id="password" className="m3-input" type="password" placeholder="Minimum 8 characters"
                        value={password} onChange={e => setPassword(e.target.value)}
                        required minLength={8} autoComplete={isLogin ? 'current-password' : 'new-password'} />
                </div>

                {!isLogin && (
                    <div className="m3-field">
                        <label className="m3-field__label" htmlFor="persona">Coach Persona</label>
                        <select id="persona" className="m3-select" value={tonePersona}
                            onChange={e => setTonePersona(e.target.value)}>
                            <option value="Motivational Coach">💪 Motivational Coach</option>
                            <option value="Drill Sergeant">🫡 Drill Sergeant</option>
                            <option value="Calm Yoga Guide">🧘 Calm Yoga Guide</option>
                            <option value="Hype Bro">🔥 Hype Bro</option>
                            <option value="Strict Professor">📚 Strict Professor</option>
                        </select>
                    </div>
                )}

                {error && <div className="m3-error-banner">{error}</div>}

                <button type="submit" className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg"
                    disabled={loading}>
                    {loading ? 'Processing...' : isLogin ? 'Sign In' : 'Create Account'}
                </button>

                <div className="auth-toggle">
                    <span>{isLogin ? "Don't have an account? " : 'Already have an account? '}</span>
                    <button type="button" onClick={() => { setIsLogin(!isLogin); setError(''); }}>
                        {isLogin ? 'Sign up' : 'Sign in'}
                    </button>
                </div>
            </form>
        </div>
    );
}
