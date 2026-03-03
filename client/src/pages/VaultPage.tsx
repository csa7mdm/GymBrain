import { useState } from 'react';
import { vaultApiKey } from '../services/api';

interface VaultPageProps {
    onComplete: () => void;
    onSkip: () => void;
}

export default function VaultPage({ onComplete, onSkip }: VaultPageProps) {
    const [provider, setProvider] = useState('openai');
    const [apiKey, setApiKey] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setLoading(true);

        const result = await vaultApiKey(provider, apiKey);
        setLoading(false);

        if (result.error) {
            setError(result.error);
            return;
        }

        setSuccess('API key securely vaulted with AES-256 encryption! 🔐');
        setApiKey('');
        setTimeout(onComplete, 1500);
    };

    return (
        <div className="page">
            <div className="brand-header">
                <div className="brand-logo">🔐</div>
                <h1 className="brand-title">Vault Setup</h1>
                <p className="brand-subtitle">Bring Your Own API Key</p>
            </div>

            <div className="glass-card">
                <form className="form-stack" onSubmit={handleSubmit}>
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.5 }}>
                        Your API key is encrypted with <strong style={{ color: 'var(--accent-secondary)' }}>AES-256-CBC</strong> before
                        storage. We never see, log, or transmit your key in plaintext.
                    </p>

                    <div className="input-group">
                        <label htmlFor="provider">Provider</label>
                        <select
                            id="provider"
                            className="input"
                            value={provider}
                            onChange={(e) => setProvider(e.target.value)}
                        >
                            <option value="openai">OpenAI</option>
                            <option value="anthropic">Anthropic</option>
                        </select>
                    </div>

                    <div className="input-group">
                        <label htmlFor="apikey">API Key</label>
                        <input
                            id="apikey"
                            className="input"
                            type="password"
                            placeholder="sk-proj-..."
                            value={apiKey}
                            onChange={(e) => setApiKey(e.target.value)}
                            required
                            minLength={10}
                            autoComplete="off"
                        />
                    </div>

                    {error && <div className="error-message">{error}</div>}
                    {success && <div className="success-message">{success}</div>}

                    <button
                        type="submit"
                        className="btn btn-primary btn-full"
                        disabled={loading || !apiKey}
                    >
                        {loading ? 'Encrypting & Storing...' : '🔒 Vault My Key'}
                    </button>

                    <button
                        type="button"
                        className="btn btn-ghost btn-full"
                        onClick={onSkip}
                    >
                        Skip for now →
                    </button>
                </form>
            </div>

            <div className="glass-card" style={{ marginTop: 16, padding: 16 }}>
                <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
                    <span style={{ fontSize: '1.2rem' }}>🛡️</span>
                    <div>
                        <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', lineHeight: 1.5 }}>
                            <strong style={{ color: 'var(--text-primary)' }}>Egypt Law 151 Compliant</strong><br />
                            Unique IV per encryption • Keys never logged • Tamper detection enabled
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
