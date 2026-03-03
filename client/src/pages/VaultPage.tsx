import { useState, useEffect } from 'react';
import { vaultApiKey, getLlmModels } from '../services/api';
import type { ILlmModel } from '../services/api';

interface VaultPageProps {
    onComplete: () => void;
    onSkip: () => void;
}

export default function VaultPage({ onComplete, onSkip }: VaultPageProps) {
    const [models, setModels] = useState<ILlmModel[]>([]);
    const [provider, setProvider] = useState('openai');
    const [model, setModel] = useState('');
    const [apiKey, setApiKey] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchModels = async () => {
            const result = await getLlmModels();
            if (result.data) {
                setModels(result.data);
                // Default to first model for openai
                const defaultModel = result.data.find(m => m.provider === 'openai')?.modelId;
                if (defaultModel) setModel(defaultModel);
            }
        };
        fetchModels();
    }, []);

    const handleProviderChange = (newProvider: string) => {
        setProvider(newProvider);
        const firstModel = models.find(m => m.provider === newProvider)?.modelId;
        if (firstModel) setModel(firstModel);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setLoading(true);

        const result = await vaultApiKey(provider, apiKey, model);
        setLoading(false);

        if (result.error) {
            setError(result.error);
            return;
        }

        setSuccess(`API key securely vaulted! Preferred: ${model} 🔐`);
        setApiKey('');
        setTimeout(onComplete, 1500);
    };

    const uniqueProviders = Array.from(new Set(models.map(m => m.provider)));
    const filteredModels = models.filter(m => m.provider === provider);

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
                        storage. Supports <strong style={{ color: 'var(--accent-primary)' }}>Groq & OpenRouter</strong> Free models.
                    </p>

                    <div className="input-group">
                        <label htmlFor="provider">LLM Provider</label>
                        <select
                            id="provider"
                            className="input"
                            value={provider}
                            onChange={(e) => handleProviderChange(e.target.value)}
                        >
                            {uniqueProviders.length > 0 ? (
                                uniqueProviders.map(p => (
                                    <option key={p} value={p}>{p.toUpperCase()}</option>
                                ))
                            ) : (
                                <>
                                    <option value="openai">OPENAI</option>
                                    <option value="groq">GROQ (Free)</option>
                                    <option value="openrouter">OPENROUTER (Free)</option>
                                    <option value="anthropic">ANTHROPIC</option>
                                </>
                            )}
                        </select>
                    </div>

                    <div className="input-group">
                        <label htmlFor="model">Model (Free models available)</label>
                        <select
                            id="model"
                            className="input"
                            value={model}
                            onChange={(e) => setModel(e.target.value)}
                        >
                            {filteredModels.map(m => (
                                <option key={m.modelId} value={m.modelId}>
                                    {m.isFree ? '🎁 ' : ''}{m.displayName} — {m.description.split(' · ')[0]}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="input-group">
                        <label htmlFor="apikey">API Key</label>
                        <input
                            id="apikey"
                            className="input"
                            type="password"
                            placeholder={provider === 'openai' ? 'sk-proj-...' : 'sk-groq-...' || 'sk-...'}
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
