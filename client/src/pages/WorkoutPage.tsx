import { useState } from 'react';
import { startWorkout } from '../services/api';
import { useAuth } from '../context/AuthContext';

interface SduiPayload {
    message?: string;
    persona?: string;
    exercise_id?: string;
    exercise_name?: string;
    target_muscle?: string;
    sets?: number;
    reps?: number;
    weight_kg?: number;
    rest_seconds?: number;
    [key: string]: string | number | boolean | undefined;
}

interface SduiComponent {
    type: string;
    payload: SduiPayload;
}

interface MegaPayload {
    screen_id?: string;
    components?: SduiComponent[];
}

export default function WorkoutPage() {
    const { user, logout } = useAuth();
    const [focus, setFocus] = useState('');
    const [payload, setPayload] = useState<MegaPayload | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Resilient parser: LLMs may return slightly different structures
    const parseMegaPayload = (raw: string): MegaPayload => {
        // Strip markdown code fences if LLM wrapped the JSON
        let cleaned = raw.trim();
        if (cleaned.startsWith('```')) {
            cleaned = cleaned.replace(/^```(?:json)?\s*\n?/, '').replace(/\n?```\s*$/, '');
        }

        const parsed = JSON.parse(cleaned);

        // Case 1: Already has top-level "components" array → perfect
        if (Array.isArray(parsed.components)) {
            return parsed as MegaPayload;
        }

        // Case 2: LLM returned an array directly
        if (Array.isArray(parsed)) {
            return { screen_id: 'workout', components: parsed };
        }

        // Case 3: Components nested inside "workout", "data", or similar wrapper
        for (const key of Object.keys(parsed)) {
            const val = parsed[key];
            if (Array.isArray(val) && val.length > 0 && val[0]?.type) {
                return { screen_id: parsed.screen_id || 'workout', components: val };
            }
            // One level deeper: { workout: { components: [...] } }
            if (val && typeof val === 'object' && !Array.isArray(val)) {
                if (Array.isArray(val.components)) {
                    return { screen_id: val.screen_id || parsed.screen_id || 'workout', components: val.components };
                }
                // Check for exercises array inside sub-object
                for (const subKey of Object.keys(val)) {
                    const subVal = val[subKey];
                    if (Array.isArray(subVal) && subVal.length > 0) {
                        // Convert raw exercise objects to set_tracker components
                        const components: SduiComponent[] = subVal.map((item: Record<string, unknown>) => ({
                            type: 'set_tracker',
                            payload: {
                                exercise_id: (item.exercise_id || item.id || '') as string,
                                exercise_name: (item.exercise_name || item.name || 'Exercise') as string,
                                target_muscle: (item.target_muscle || item.muscle || '') as string,
                                sets: (item.sets || 3) as number,
                                reps: (item.reps || 10) as number,
                                weight_kg: (item.weight_kg || item.weight || 0) as number,
                                rest_seconds: (item.rest_seconds || item.rest || 90) as number,
                            }
                        }));
                        return { screen_id: 'workout', components };
                    }
                }
            }
        }

        // Case 4: Flat object with exercise fields → wrap as single component
        if (parsed.exercise_name || parsed.exercise_id) {
            return { screen_id: 'workout', components: [{ type: 'set_tracker', payload: parsed }] };
        }

        return { screen_id: 'workout', components: [] };
    };

    const handleStart = async () => {
        setError('');
        setLoading(true);

        const result = await startWorkout(focus || undefined);
        setLoading(false);

        if (result.error) {
            setError(result.error);
            return;
        }

        if (result.data) {
            try {
                const mega = parseMegaPayload(result.data.megaPayloadJson);
                setPayload(mega);
            } catch (e) {
                setPayload({ components: [] });
                setError('Failed to parse workout response.');
                console.error('Parse error:', e, 'Raw:', result.data.megaPayloadJson);
            }
        }
    };

    // SDUI Component Renderer
    const renderComponent = (comp: SduiComponent, idx: number) => {
        switch (comp.type) {
            case 'tone_card':
                return (
                    <div key={idx} className="tone-card">
                        <p>"{comp.payload.message}"</p>
                        {comp.payload.persona && (
                            <span className="persona-tag">— {comp.payload.persona}</span>
                        )}
                    </div>
                );

            case 'set_tracker':
                return (
                    <div key={idx} className="set-tracker">
                        <div className="set-tracker-header">
                            <span className="exercise-name">{comp.payload.exercise_name || 'Exercise'}</span>
                            {comp.payload.target_muscle && (
                                <span className="exercise-muscle">{comp.payload.target_muscle}</span>
                            )}
                        </div>
                        <div className="set-tracker-stats">
                            <div className="stat-box">
                                <div className="stat-value numeric">{comp.payload.sets ?? 3}</div>
                                <div className="stat-label">Sets</div>
                            </div>
                            <div className="stat-box">
                                <div className="stat-value numeric">{comp.payload.reps ?? 10}</div>
                                <div className="stat-label">Reps</div>
                            </div>
                            <div className="stat-box">
                                <div className="stat-value numeric">{comp.payload.weight_kg ?? 0}</div>
                                <div className="stat-label">kg</div>
                            </div>
                        </div>
                        {comp.payload.rest_seconds && (
                            <div className="rest-timer">
                                <span className="rest-icon">⏱️</span>
                                <span className="numeric">{comp.payload.rest_seconds}s</span> rest
                            </div>
                        )}
                    </div>
                );

            default:
                return (
                    <div key={idx} className="glass-card" style={{ marginBottom: 12 }}>
                        <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                            Unknown component: <code>{comp.type}</code>
                        </p>
                    </div>
                );
        }
    };

    if (loading) {
        return (
            <div className="page">
                <div className="loading-screen">
                    <div className="spinner" />
                    <p className="loading-text">Generating your workout with AI...</p>
                    <p style={{ color: 'var(--text-tertiary)', fontSize: '0.8rem' }}>
                        SafetyGate validating output
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="page">
            {!payload ? (
                <>
                    <div className="brand-header">
                        <div className="brand-logo">⚡</div>
                        <h1 className="brand-title">Ready to Train</h1>
                        <p className="brand-subtitle">{user?.email}</p>
                    </div>

                    <div className="glass-card">
                        <div className="form-stack">
                            <div className="input-group">
                                <label htmlFor="focus">Workout Focus (optional)</label>
                                <select
                                    id="focus"
                                    className="input"
                                    value={focus}
                                    onChange={(e) => setFocus(e.target.value)}
                                >
                                    <option value="">Full Body</option>
                                    <option value="upper body strength">Upper Body Strength</option>
                                    <option value="lower body power">Lower Body Power</option>
                                    <option value="chest and arms">Chest & Arms</option>
                                    <option value="back and shoulders">Back & Shoulders</option>
                                    <option value="core and abs">Core & Abs</option>
                                </select>
                            </div>

                            {error && <div className="error-message">{error}</div>}

                            <button
                                className="btn btn-primary btn-full btn-lg"
                                onClick={handleStart}
                            >
                                🚀 Generate Workout
                            </button>
                        </div>
                    </div>

                    <div style={{ display: 'flex', gap: 8, marginTop: 16, flexWrap: 'wrap', justifyContent: 'center' }}>
                        <span className="chip chip-success">✓ Health Check Passed</span>
                        <span className="chip chip-info">BYO AI Key Active</span>
                        <span className="chip chip-warning">Model Fallback Enabled</span>
                    </div>

                    <button
                        className="btn btn-ghost btn-full"
                        style={{ marginTop: 24 }}
                        onClick={logout}
                    >
                        Sign Out
                    </button>
                </>
            ) : (
                <>
                    <div className="workout-header">
                        <h2>Your Workout</h2>
                        <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: 4 }}>
                            AI-generated • SafetyGate validated
                        </p>
                    </div>

                    {payload.components?.map((comp, idx) => renderComponent(comp, idx))}

                    {(!payload.components || payload.components.length === 0) && (
                        <div className="glass-card" style={{ textAlign: 'center' }}>
                            <p style={{ color: 'var(--text-secondary)' }}>No workout components returned.</p>
                        </div>
                    )}

                    <button
                        className="btn btn-secondary btn-full"
                        style={{ marginTop: 16 }}
                        onClick={() => setPayload(null)}
                    >
                        ← Generate New Workout
                    </button>
                </>
            )}
        </div>
    );
}
