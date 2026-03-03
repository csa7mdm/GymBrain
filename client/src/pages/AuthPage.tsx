import { useState } from 'react';
import { login, register } from '../services/api';
import { useAuth } from '../context/AuthContext';

interface AuthPageProps {
    onSuccess: () => void;
}

export default function AuthPage({ onSuccess }: AuthPageProps) {
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

        if (result.error) {
            setError(result.error);
            return;
        }

        if (result.data) {
            setAuth(result.data.userId, email, result.data.token);
            onSuccess();
        }
    };

    return (
        <div className="page">
            <div className="brand-header">
                <div className="brand-logo">🧠</div>
                <h1 className="brand-title">GymBrain</h1>
                <p className="brand-subtitle">AI-Powered Fitness Coaching</p>
            </div>

            <div className="glass-card">
                <form className="form-stack" onSubmit={handleSubmit}>
                    <h2 style={{ textAlign: 'center', marginBottom: 8 }}>
                        {isLogin ? 'Welcome Back' : 'Create Account'}
                    </h2>

                    <div className="input-group">
                        <label htmlFor="email">Email</label>
                        <input
                            id="email"
                            className="input"
                            type="email"
                            placeholder="you@example.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            autoComplete="email"
                        />
                    </div>

                    <div className="input-group">
                        <label htmlFor="password">Password</label>
                        <input
                            id="password"
                            className="input"
                            type="password"
                            placeholder="Minimum 8 characters"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            minLength={8}
                            autoComplete={isLogin ? 'current-password' : 'new-password'}
                        />
                    </div>

                    {!isLogin && (
                        <div className="input-group">
                            <label htmlFor="persona">Tone Persona</label>
                            <select
                                id="persona"
                                className="input"
                                value={tonePersona}
                                onChange={(e) => setTonePersona(e.target.value)}
                            >
                                <option value="Motivational Coach">💪 Motivational Coach</option>
                                <option value="Drill Sergeant">🫡 Drill Sergeant</option>
                                <option value="Calm Yoga Guide">🧘 Calm Yoga Guide</option>
                                <option value="Hype Bro">🔥 Hype Bro</option>
                                <option value="Strict Professor">📚 Strict Professor</option>
                            </select>
                        </div>
                    )}

                    {error && <div className="error-message">{error}</div>}

                    <button
                        type="submit"
                        className="btn btn-primary btn-full btn-lg"
                        disabled={loading}
                    >
                        {loading ? 'Processing...' : isLogin ? 'Sign In' : 'Create Account'}
                    </button>

                    <div className="divider">or</div>

                    <button
                        type="button"
                        className="btn btn-ghost btn-full"
                        onClick={() => { setIsLogin(!isLogin); setError(''); }}
                    >
                        {isLogin ? "Don't have an account? Sign up" : 'Already have an account? Sign in'}
                    </button>
                </form>
            </div>
        </div>
    );
}
