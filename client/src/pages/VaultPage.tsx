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
        <div className="app-content app-content--no-pad fade-in">
            <div style={{ textAlign: 'center', marginBottom: 24, marginTop: 16 }}>
                <div style={{ fontSize: '2.5rem', marginBottom: 8 }}>🔐</div>
                <h2 className="md-headline-sm" style={{ color: 'var(--md-primary)' }}>Secure Vault</h2>
                <p className="md-body-md text-muted">Bring Your Own API Key</p>
            </div>

            <div className="m3-card">
                <form onSubmit={handleSubmit}>
                    <p className="md-body-sm mb-md" style={{ color: 'var(--md-on-surface-variant)', lineHeight: 1.6 }}>
                        Your API key is encrypted with <strong className="text-secondary">AES-256-CBC</strong> before
                        storage. Supports <strong className="text-primary">Groq & OpenRouter</strong> Free models.
                    </p>

                    <div className="m3-field">
                        <label className="m3-field__label" htmlFor="provider">LLM Provider</label>
                        <select
                            id="provider"
                            className="m3-select"
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

                    <div className="m3-field">
                        <label className="m3-field__label" htmlFor="model">Model Preference</label>
                        <select
                            id="model"
                            className="m3-select"
                            value={model}
                            onChange={(e) => setModel(e.target.value)}
                        >
                            {filteredModels.map(m => (
                                <option key={m.modelId} value={m.modelId}>
                                    {m.isFree ? '🎁 ' : ''}{m.displayName}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="m3-field">
                        <label className="m3-field__label" htmlFor="apikey">API Key</label>
                        <input
                            id="apikey"
                            className="m3-input"
                            type="password"
                            placeholder={provider === 'openai' ? 'sk-proj-...' : provider === 'groq' ? 'gsk_...' : 'sk-...'}
                            value={apiKey}
                            onChange={(e) => setApiKey(e.target.value)}
                            required
                            minLength={10}
                            autoComplete="off"
                        />
                    </div>

                    {error && <div className="m3-error-banner mb-md">{error}</div>}
                    {success && <div className="m3-success-banner mb-md" style={{ color: 'var(--md-success)', padding: 12, background: 'var(--md-success-container)', borderRadius: 8 }}>{success}</div>}

                    <button
                        type="submit"
                        className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg"
                        disabled={loading || !apiKey}
                    >
                        {loading ? '🔐 Encrypting...' : '🔒 Vault My Key'}
                    </button>

                    <button
                        type="button"
                        className="m3-btn m3-btn--text m3-btn--full mt-sm"
                        onClick={onSkip}
                    >
                        Skip for now →
                    </button>
                </form>
            </div>

            <div className="m3-card mt-md" style={{ background: 'var(--md-surface-container-low)' }}>
                <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start' }}>
                    <span style={{ fontSize: '1.2rem' }}>🛡️</span>
                    <div>
                        <p className="md-body-sm" style={{ color: 'var(--md-on-surface-variant)', lineHeight: 1.5 }}>
                            <strong className="text-primary">Egypt Law 151 Compliant</strong><br />
                            Unique IV per encryption • Keys never logged • Tamper detection enabled
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
