import { useState } from 'react';
import { vaultApiKey, discoverLlmModels, type ILlmModel } from '../services/api';

interface VaultPageProps { onComplete: () => void; onSkip: () => void; }
export default function VaultPage({ onComplete, onSkip }: VaultPageProps) {
  const [provider, setProvider] = useState('openrouter');
  const [apiKey, setApiKey] = useState('');
  const [models, setModels] = useState<ILlmModel[]>([]);
  const [model, setModel] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [loaded, setLoaded] = useState(false);
  const [success, setSuccess] = useState('');
  const invalidate = () => { setModels([]); setModel(''); setLoaded(false); setError(''); setSuccess(''); };
  const refresh = async () => {
    setBusy(true); setError(''); setSuccess(''); setLoaded(false); setModels([]); setModel('');
    const result = await discoverLlmModels(provider, apiKey.trim());
    setBusy(false);
    if (result.error || !result.data) { setError(result.error || 'Could not load models. Please retry.'); return; }
    setModels(result.data); setModel(result.data[0]?.modelId || ''); setLoaded(true);
  };
  const save = async (event: React.FormEvent) => {
    event.preventDefault();
    if (busy || !loaded || !model) return;
    setBusy(true); setError('');
    const result = await vaultApiKey(provider, apiKey.trim(), model);
    setBusy(false);
    if (result.error) { setError(result.error); return; }
    setApiKey(''); setModels([]); setModel(''); setLoaded(false);
    setSuccess(result.data?.message || 'Provider settings saved.');
  };
  return <div className="app-content fade-in">
    <h2 className="md-headline-sm">Your AI connection</h2>
    <p className="md-body-md text-muted mb-md">Choose a provider, load its current models, then save your choice.</p>
    <form className="m3-card" onSubmit={save}>
      <div className="m3-field"><label className="m3-field__label" htmlFor="provider">Provider</label>
        <select id="provider" className="m3-select" value={provider} disabled={busy} onChange={e => { setProvider(e.target.value); setApiKey(''); invalidate(); }}>
          <option value="openrouter">OpenRouter · free models</option><option value="groq">Groq</option><option value="openai">OpenAI</option>
        </select></div>
      <div className="m3-field"><label className="m3-field__label" htmlFor="apikey">API key</label>
        <input id="apikey" type="password" className="m3-input" value={apiKey} disabled={busy} autoComplete="off" onChange={e => { setApiKey(e.target.value); invalidate(); }} /></div>
      <p className="md-body-sm text-muted mb-md">Loading models sends this key to GymBrain and your selected provider for verification. It does not generate content. Saving stores the key encrypted on the server.</p>
      <button type="button" className="m3-btn m3-btn--outlined m3-btn--full" disabled={busy || apiKey.trim().length < 10} onClick={refresh}>{busy ? 'Connecting…' : 'Load latest models'}</button>
      {loaded && <p role="status" className="md-body-sm mt-md">{models.length ? `${models.length} models loaded from the provider. Newest listed first.` : 'No compatible models are currently available. Try refreshing later.'}</p>}
      {models.length > 0 && <div className="m3-field mt-md"><label className="m3-field__label" htmlFor="model">Model</label>
        <select id="model" className="m3-select" value={model} disabled={busy} onChange={e => setModel(e.target.value)}>{models.map(m => <option key={m.modelId} value={m.modelId}>{m.isFree ? 'Free · ' : ''}{m.displayName}</option>)}</select></div>}
      <p className="md-body-sm text-muted mt-md">{provider === 'openrouter' ? 'Only free models advertising JSON output are shown. Free endpoints can still have quotas or be temporarily unavailable.' : 'Your provider’s pricing and account limits apply. Listed models may have different generation capabilities.'}</p>
      {error && <p role="alert" className="m3-error-banner">{error}</p>}
      {success && <p role="status" className="m3-success-banner">{success}</p>}
      <button className="m3-btn m3-btn--filled m3-btn--full mt-md" disabled={busy || !loaded || !model}>Save connection</button>
      {success && <button type="button" className="m3-btn m3-btn--tonal m3-btn--full mt-md" onClick={onComplete}>Done</button>}
      <button type="button" className="m3-btn m3-btn--text m3-btn--full mt-md" onClick={onSkip}>Back to profile</button>
    </form>
  </div>;
}
