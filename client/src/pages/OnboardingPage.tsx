import { useState, useEffect } from 'react';
import { vaultApiKey, getLlmModels } from '../services/api';
import type { ILlmModel } from '../services/api';

interface OnboardingProps {
    onComplete: (profile: UserProfile) => void;
}

export interface UserProfile {
    name: string; age: number; height: number; weight: number;
    goal: string; level: string; daysPerWeek: number;
    equipment: string[]; focusAreas: string[];
    injuries: string; // free-text for InjuryFilter backend
    provider: string; model: string; apiKey: string;
}

const GOALS = [
    { id: 'muscle', icon: '💪', label: 'Build Muscle' },
    { id: 'fat-loss', icon: '🔥', label: 'Lose Fat' },
    { id: 'strength', icon: '🏋️', label: 'Get Strong' },
    { id: 'endurance', icon: '🏃', label: 'Endurance' },
    { id: 'health', icon: '❤️', label: 'Stay Healthy' },
];

const LEVELS = ['Beginner', 'Intermediate', 'Advanced', 'Athlete'];
const DAYS = [2, 3, 4, 5, 6];

const EQUIPMENT = [
    'Barbell', 'Dumbbells', 'Cables', 'Machines',
    'Kettlebells', 'Bands', 'Bodyweight', 'Pull-up Bar',
];

const FOCUS_AREAS = [
    'Chest', 'Back', 'Shoulders', 'Arms',
    'Legs', 'Core', 'Glutes', 'Full Body',
];

export default function OnboardingPage({ onComplete }: OnboardingProps) {
    const [step, setStep] = useState(1);

    // Step 1
    const [name, setName] = useState('');
    const [age, setAge] = useState(28);
    const [height, setHeight] = useState(175);
    const [weight, setWeight] = useState(78);

    // Step 2
    const [goal, setGoal] = useState('muscle');
    const [level, setLevel] = useState('Intermediate');
    const [daysPerWeek, setDaysPerWeek] = useState(4);
    const [equipment, setEquipment] = useState<string[]>(['Dumbbells', 'Bodyweight']);
    const [focusAreas, setFocusAreas] = useState<string[]>(['Full Body']);
    const [injuries, setInjuries] = useState('');

    // Step 3
    const [useFree, setUseFree] = useState(true);
    const [provider, setProvider] = useState('groq');
    const [model, setModel] = useState('llama-3.3-70b-versatile');
    const [apiKey, setApiKey] = useState('');
    const [models, setModels] = useState<ILlmModel[]>([]);
    const [vaultLoading, setVaultLoading] = useState(false);
    const [vaultError, setVaultError] = useState('');

    useEffect(() => {
        getLlmModels().then(r => { if (r.data) setModels(r.data); });
    }, []);

    const toggleChip = (list: string[], item: string, setter: (v: string[]) => void) => {
        setter(list.includes(item) ? list.filter(x => x !== item) : [...list, item]);
    };

    const filteredModels = models.filter(m => m.provider === provider);

    const handleFinish = async () => {
        try {
            const profile: UserProfile = {
                name: name || 'Athlete',
                age, height, weight, goal, level, daysPerWeek,
                equipment, focusAreas, injuries,
                provider: useFree ? 'groq' : provider,
                model: useFree ? 'llama-3.3-70b-versatile' : model,
                apiKey: useFree ? '' : apiKey,
            };

            if (!useFree && apiKey.trim()) {
                setVaultLoading(true);
                setVaultError('');
                try {
                    const result = await vaultApiKey(provider, apiKey, model);
                    setVaultLoading(false);
                    if (result.error) { setVaultError(result.error); return; }
                } catch (e) {
                    setVaultLoading(false);
                    console.warn('Vault API key failed, continuing anyway:', e);
                }
            }

            const profileData = JSON.stringify(profile);
            localStorage.setItem('gymbrain_profile', profileData);
            localStorage.getItem('gymbrain_profile'); // Ensure flushed
            console.log('Profile saved, completing onboarding...');

            setTimeout(() => onComplete(profile), 50);
        } catch (err) {
            console.error('handleFinish error:', err);
            // Force complete even on error
            const fallbackProfile: UserProfile = {
                name: name || 'Athlete', age, height, weight, goal, level,
                daysPerWeek, equipment, focusAreas, injuries,
                provider: 'groq', model: 'llama-3.3-70b-versatile', apiKey: '',
            };
            localStorage.setItem('gymbrain_profile', JSON.stringify(fallbackProfile));
            localStorage.getItem('gymbrain_profile'); // Ensure flushed
            setTimeout(() => onComplete(fallbackProfile), 50);
        }
    };

    const canContinue1 = name.trim().length > 0;
    const canFinish = useFree || apiKey.trim().length > 0;

    const stepPercent = ((step - 1) / 2) * 100;

    return (
        <div className="onboarding fade-in">
            {/* Stepper */}
            <div className="m3-stepper">
                {[1, 2, 3].map((s, i) => (
                    <div key={s} className="m3-stepper__step">
                        {i > 0 && <div className={`m3-stepper__line ${step > s - 1 ? 'm3-stepper__line--done' : ''}`} />}
                        <div className={`m3-stepper__circle ${step === s ? 'm3-stepper__circle--active' : step > s ? 'm3-stepper__circle--done' : 'm3-stepper__circle--pending'}`}>
                            {step > s ? '✓' : s}
                        </div>
                    </div>
                ))}
            </div>
            <div className="m3-stepper__progress">
                <div className="m3-stepper__progress-fill" style={{ width: `${stepPercent}%` }} />
            </div>

            <div className="onboarding__content">
                {/* ─── STEP 1: About You ─── */}
                {step === 1 && (
                    <div className="slide-up">
                        <div className="onboarding__header">
                            <span className="onboarding__emoji">👋</span>
                            <h2 className="onboarding__title">Let's get to know you</h2>
                            <p className="onboarding__desc">This helps us personalize your workouts</p>
                        </div>

                        <div className="m3-field">
                            <label className="m3-field__label" htmlFor="ob-name">Your Name</label>
                            <input id="ob-name" className="m3-input" placeholder="What should we call you?"
                                value={name} onChange={e => setName(e.target.value)} autoFocus />
                        </div>

                        <div className="m3-slider-group">
                            <div className="m3-slider-group__header">
                                <span className="m3-slider-group__label">Age</span>
                                <span className="m3-slider-group__value">{age} years</span>
                            </div>
                            <input type="range" className="m3-slider" min={14} max={80} value={age}
                                onChange={e => setAge(+e.target.value)} />
                        </div>

                        <div className="m3-slider-group">
                            <div className="m3-slider-group__header">
                                <span className="m3-slider-group__label">Height</span>
                                <span className="m3-slider-group__value">{height} cm</span>
                            </div>
                            <input type="range" className="m3-slider" min={140} max={220} value={height}
                                onChange={e => setHeight(+e.target.value)} />
                        </div>

                        <div className="m3-slider-group">
                            <div className="m3-slider-group__header">
                                <span className="m3-slider-group__label">Weight</span>
                                <span className="m3-slider-group__value">{weight} kg</span>
                            </div>
                            <input type="range" className="m3-slider" min={35} max={200} value={weight}
                                onChange={e => setWeight(+e.target.value)} />
                        </div>
                    </div>
                )}

                {/* ─── STEP 2: Your Goals ─── */}
                {step === 2 && (
                    <div className="slide-up">
                        <div className="onboarding__header">
                            <span className="onboarding__emoji">🎯</span>
                            <h2 className="onboarding__title">What's your goal?</h2>
                            <p className="onboarding__desc">We'll tailor every workout to match</p>
                        </div>

                        <div className="section-header">
                            <span className="section-header__icon">🏆</span>
                            <span className="section-header__title">Primary Goal</span>
                        </div>
                        <div className="m3-segmented mb-md">
                            {GOALS.slice(0, 3).map(g => (
                                <button key={g.id} className={`m3-segmented__btn ${goal === g.id ? 'm3-segmented__btn--active' : ''}`}
                                    onClick={() => setGoal(g.id)}>
                                    <span className="m3-segmented__icon">{g.icon}</span> {g.label}
                                </button>
                            ))}
                        </div>
                        <div className="m3-segmented mb-md">
                            {GOALS.slice(3).map(g => (
                                <button key={g.id} className={`m3-segmented__btn ${goal === g.id ? 'm3-segmented__btn--active' : ''}`}
                                    onClick={() => setGoal(g.id)}>
                                    <span className="m3-segmented__icon">{g.icon}</span> {g.label}
                                </button>
                            ))}
                        </div>

                        <div className="section-header">
                            <span className="section-header__icon">📊</span>
                            <span className="section-header__title">Fitness Level</span>
                        </div>
                        <div className="m3-segmented mb-md">
                            {LEVELS.map(l => (
                                <button key={l} className={`m3-segmented__btn ${level === l ? 'm3-segmented__btn--active' : ''}`}
                                    onClick={() => setLevel(l)}>
                                    {l}
                                </button>
                            ))}
                        </div>

                        <div className="section-header">
                            <span className="section-header__icon">📅</span>
                            <span className="section-header__title">Training Days / Week</span>
                        </div>
                        <div className="m3-segmented mb-md">
                            {DAYS.map(d => (
                                <button key={d} className={`m3-segmented__btn ${daysPerWeek === d ? 'm3-segmented__btn--active' : ''}`}
                                    onClick={() => setDaysPerWeek(d)}>
                                    {d}x
                                </button>
                            ))}
                        </div>

                        <div className="section-header">
                            <span className="section-header__icon">🏠</span>
                            <span className="section-header__title">Available Equipment</span>
                        </div>
                        <div className="m3-chips mb-md">
                            {EQUIPMENT.map(eq => (
                                <button key={eq} className={`m3-chip ${equipment.includes(eq) ? 'm3-chip--selected' : ''}`}
                                    onClick={() => toggleChip(equipment, eq, setEquipment)}>
                                    {eq}
                                </button>
                            ))}
                        </div>

                        <div className="section-header">
                            <span className="section-header__icon">💪</span>
                            <span className="section-header__title">Focus Areas</span>
                        </div>
                        <div className="m3-chips">
                            {FOCUS_AREAS.map(fa => (
                                <button key={fa} className={`m3-chip ${focusAreas.includes(fa) ? 'm3-chip--selected' : ''}`}
                                    onClick={() => toggleChip(focusAreas, fa, setFocusAreas)}>
                                    {fa}
                                </button>
                            ))}
                        </div>

                        {/* Injuries / Conditions — critical for InjuryFilter backend */}
                        <div className="section-header" style={{ marginTop: 20 }}>
                            <span className="section-header__icon">🛡️</span>
                            <span className="section-header__title">Any injuries or limitations?</span>
                        </div>
                        <p style={{ color: '#aaa', fontSize: 12, marginBottom: 8, marginLeft: 2 }}>
                            We'll automatically avoid exercises that could aggravate them.
                        </p>
                        <div className="m3-field">
                            <textarea
                                id="ob-injuries"
                                className="m3-input"
                                placeholder="e.g., bad knees, lower back pain, shoulder impingement — or leave blank if none"
                                rows={2}
                                style={{ resize: 'none', fontFamily: 'inherit', fontSize: 14 }}
                                value={injuries}
                                onChange={e => setInjuries(e.target.value)}
                            />
                        </div>
                    </div>
                )}


                {/* ─── STEP 3: AI Setup ─── */}
                {step === 3 && (
                    <div className="slide-up">
                        <div className="onboarding__header">
                            <span className="onboarding__emoji">🤖</span>
                            <h2 className="onboarding__title">AI Coach Setup</h2>
                            <p className="onboarding__desc">Choose how to power your workouts</p>
                        </div>

                        <div className={`m3-card ${useFree ? 'm3-card--recommended' : ''}`}
                            onClick={() => setUseFree(true)} style={{ cursor: 'pointer', marginBottom: 12 }}>
                            <div className="flex items-center gap-md" style={{ gap: 12 }}>
                                <span style={{ fontSize: '1.5rem' }}>✨</span>
                                <div style={{ flex: 1 }}>
                                    <div className="md-title-md">Use Free AI</div>
                                    <div className="md-body-sm text-muted">Powered by Llama 3.3 — No API key needed</div>
                                </div>
                                {useFree && <span style={{ fontSize: '1.2rem' }}>✓</span>}
                            </div>
                        </div>

                        <div className="m3-divider-text">or bring your own key</div>

                        <div className={`m3-card ${!useFree ? 'm3-card--outlined' : ''}`}
                            onClick={() => setUseFree(false)} style={{ cursor: 'pointer', opacity: useFree ? 0.5 : 1, transition: 'opacity 0.3s' }}>
                            <div className="flex items-center gap-md" style={{ gap: 12, marginBottom: useFree ? 0 : 12 }}>
                                <span style={{ fontSize: '1.2rem' }}>🔑</span>
                                <div style={{ flex: 1 }}>
                                    <div className="md-title-md">BYO API Key</div>
                                    <div className="md-body-sm text-muted">Use your own provider & model</div>
                                </div>
                                {!useFree && <span style={{ fontSize: '1.2rem' }}>✓</span>}
                            </div>

                            {!useFree && (
                                <div style={{ marginTop: 12 }} onClick={e => e.stopPropagation()}>
                                    <div className="m3-field">
                                        <label className="m3-field__label" htmlFor="ob-provider">Provider</label>
                                        <select id="ob-provider" className="m3-select" value={provider}
                                            onChange={e => { setProvider(e.target.value); setModel(''); }}>
                                            <option value="groq">Groq (Free)</option>
                                            <option value="openrouter">OpenRouter</option>
                                            <option value="openai">OpenAI</option>
                                            <option value="anthropic">Anthropic</option>
                                        </select>
                                    </div>
                                    <div className="m3-field">
                                        <label className="m3-field__label" htmlFor="ob-model">Model</label>
                                        <select id="ob-model" className="m3-select" value={model}
                                            onChange={e => setModel(e.target.value)}>
                                            {filteredModels.map(m => (
                                                <option key={m.modelId} value={m.modelId}>{m.displayName}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="m3-field">
                                        <label className="m3-field__label" htmlFor="ob-key">API Key</label>
                                        <input id="ob-key" className="m3-input" type="password"
                                            placeholder="sk-..." value={apiKey}
                                            onChange={e => setApiKey(e.target.value)} />
                                    </div>
                                </div>
                            )}
                        </div>

                        {vaultError && <div className="m3-error-banner mt-sm">{vaultError}</div>}
                    </div>
                )}
            </div>

            {/* Footer Navigation */}
            <div className="onboarding__footer">
                {step > 1 ? (
                    <button className="m3-btn m3-btn--outlined" onClick={() => setStep(s => s - 1)}>← Back</button>
                ) : <div />}
                {step < 3 ? (
                    <button className="m3-btn m3-btn--filled"
                        disabled={step === 1 && !canContinue1}
                        onClick={() => setStep(s => s + 1)}>
                        Continue →
                    </button>
                ) : (
                    <button className="m3-btn m3-btn--filled"
                        disabled={!canFinish || vaultLoading}
                        onClick={handleFinish}>
                        {vaultLoading ? 'Setting up...' : '🚀 Let\'s Go!'}
                    </button>
                )}
            </div>
        </div>
    );
}
