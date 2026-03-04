import { useState, useEffect, useRef, useCallback } from 'react';
import { startWorkout, saveWorkout } from '../services/api';
import { searchExercise, type ExerciseDbItem } from '../services/exerciseDb';


interface SduiPayload {
    message?: string; persona?: string; exercise_id?: string; exercise_name?: string;
    target_muscle?: string; sets?: number; reps?: number; weight_kg?: number;
    rest_seconds?: number; coach_tip?: string;
    [key: string]: string | number | boolean | undefined;
}
interface SduiComponent { type: string; payload: SduiPayload; }
interface MegaPayload { screen_id?: string; components?: SduiComponent[]; }
interface SetProgress { completed: boolean[]; actualWeight: number[]; actualReps: number[]; }

function useRestTimer() {
    const [seconds, setSeconds] = useState(0);
    const [isRunning, setIsRunning] = useState(false);
    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const start = useCallback((duration: number) => { setSeconds(duration); setIsRunning(true); }, []);
    const stop = useCallback(() => { setIsRunning(false); setSeconds(0); }, []);
    useEffect(() => {
        if (isRunning && seconds > 0) {
            intervalRef.current = setInterval(() => {
                setSeconds(s => { if (s <= 1) { setIsRunning(false); return 0; } return s - 1; });
            }, 1000);
        }
        return () => { if (intervalRef.current) clearInterval(intervalRef.current); };
    }, [isRunning, seconds]);
    return { seconds, isRunning, start, stop };
}

export default function WorkoutPage() {
    const [focus, setFocus] = useState('');
    const [payload, setPayload] = useState<MegaPayload | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [exerciseImages, setExerciseImages] = useState<Map<string, ExerciseDbItem>>(new Map());
    const [setProgress, setSetProgress] = useState<Map<number, SetProgress>>(new Map());
    const [expandedCard, setExpandedCard] = useState<number | null>(null);
    const restTimer = useRestTimer();
    const [restExerciseIdx, setRestExerciseIdx] = useState<number | null>(null);

    const parseMegaPayload = (raw: string): MegaPayload => {
        let cleaned = raw.trim();
        if (cleaned.startsWith('```')) { cleaned = cleaned.replace(/^```(?:json)?\s*\n?/, '').replace(/\n?```\s*$/, ''); }
        const parsed = JSON.parse(cleaned);
        if (Array.isArray(parsed.components)) return parsed as MegaPayload;
        if (Array.isArray(parsed)) return { screen_id: 'workout', components: parsed };
        for (const key of Object.keys(parsed)) {
            const val = parsed[key];
            if (Array.isArray(val) && val.length > 0 && val[0]?.type) return { screen_id: parsed.screen_id || 'workout', components: val };
            if (val && typeof val === 'object' && !Array.isArray(val)) {
                if (Array.isArray(val.components)) return { screen_id: val.screen_id || parsed.screen_id || 'workout', components: val.components };
                for (const subKey of Object.keys(val)) {
                    const subVal = val[subKey];
                    if (Array.isArray(subVal) && subVal.length > 0) {
                        const components: SduiComponent[] = subVal.map((item: Record<string, unknown>) => ({
                            type: 'set_tracker', payload: {
                                exercise_id: (item.exercise_id || item.id || '') as string,
                                exercise_name: (item.exercise_name || item.name || 'Exercise') as string,
                                target_muscle: (item.target_muscle || item.muscle || '') as string,
                                sets: (item.sets || 3) as number, reps: (item.reps || 10) as number,
                                weight_kg: (item.weight_kg || item.weight || 0) as number,
                                rest_seconds: (item.rest_seconds || item.rest || 90) as number,
                            }
                        }));
                        return { screen_id: 'workout', components };
                    }
                }
            }
        }
        if (parsed.exercise_name || parsed.exercise_id) return { screen_id: 'workout', components: [{ type: 'set_tracker', payload: parsed }] };
        return { screen_id: 'workout', components: [] };
    };

    useEffect(() => {
        if (!payload?.components) return;
        const names = payload.components.filter(c => c.type === 'set_tracker' && c.payload.exercise_name).map(c => c.payload.exercise_name as string);
        if (names.length === 0) return;
        const fetchImages = async () => {
            const ni = new Map(exerciseImages);
            for (const name of names) { if (!ni.has(name)) { const r = await searchExercise(name); if (r) ni.set(name, r); } }
            setExerciseImages(ni);
        };
        fetchImages();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [payload]);

    useEffect(() => {
        if (!payload?.components) return;
        const prog = new Map<number, SetProgress>();
        payload.components.forEach((comp, idx) => {
            if (comp.type === 'set_tracker') {
                const n = comp.payload.sets || 3;
                prog.set(idx, { completed: Array(n).fill(false), actualWeight: Array(n).fill(comp.payload.weight_kg || 0), actualReps: Array(n).fill(comp.payload.reps || 10) });
            }
        });
        setSetProgress(prog);
    }, [payload]);

    const handleStart = async () => {
        setError(''); setLoading(true);
        const result = await startWorkout(focus || undefined);
        setLoading(false);
        if (result.error) { setError(result.error); return; }
        if (result.data?.megaPayloadJson) { try { setPayload(parseMegaPayload(result.data.megaPayloadJson)); } catch (e) { setError(`Parse error: ${e instanceof Error ? e.message : 'Unknown'}`); } }
    };

    const toggleSet = (ei: number, si: number) => {
        setSetProgress(prev => {
            const m = new Map(prev); const p = m.get(ei);
            if (p) {
                const c = [...p.completed]; c[si] = !c[si]; m.set(ei, { ...p, completed: c });
                if (c[si]) { const comp = payload?.components?.[ei]; if (comp) { restTimer.start((comp.payload.rest_seconds || 90) as number); setRestExerciseIdx(ei); } }
            }
            return m;
        });
    };

    const getProgress = () => {
        let total = 0, done = 0;
        setProgress.forEach(p => { total += p.completed.length; done += p.completed.filter(Boolean).length; });
        return { totalSets: total, completedSets: done, percent: total > 0 ? (done / total) * 100 : 0 };
    };

    const renderExerciseCard = (comp: SduiComponent, idx: number) => {
        const progress = setProgress.get(idx);
        const exInfo = exerciseImages.get(comp.payload.exercise_name as string);
        const isExp = expandedCard === idx;
        const doneSets = progress?.completed.filter(Boolean).length || 0;
        const totalS = progress?.completed.length || (comp.payload.sets as number) || 3;
        const allDone = doneSets === totalS;
        const isResting = restTimer.isRunning && restExerciseIdx === idx;

        return (
            <div key={idx} className={`exercise-card ${allDone ? 'exercise-card--done' : ''} ${isResting ? 'exercise-card--resting' : ''}`} style={{ animationDelay: `${idx * 0.08}s` }}>
                <div className="exercise-card__progress-bar"><div className="exercise-card__progress-fill" style={{ width: `${(doneSets / totalS) * 100}%` }} /></div>
                <div className="exercise-card__header">
                    {exInfo?.gifUrl ? (
                        <div className="exercise-card__gif-container"><img src={exInfo.gifUrl} alt={comp.payload.exercise_name as string} className="exercise-card__gif" loading="lazy" /></div>
                    ) : (
                        <div className="exercise-card__gif-placeholder"><span>🏋️</span></div>
                    )}
                    <div className="exercise-card__title-block">
                        <h3 className="exercise-name">{comp.payload.exercise_name || 'Exercise'}</h3>
                        <div className="exercise-card__meta">
                            {comp.payload.target_muscle && <span className="exercise-card__muscle-tag">{comp.payload.target_muscle as string}</span>}
                            {exInfo?.equipment && <span className="exercise-card__equipment-tag">{exInfo.equipment}</span>}
                        </div>
                        {allDone && <span className="exercise-card__done-badge">✅ Complete</span>}
                    </div>
                </div>
                <div className="set-circles">
                    {progress?.completed.map((done, si) => (
                        <button key={si} className={`set-circle ${done ? 'set-circle--done' : ''}`} onClick={() => toggleSet(idx, si)} title={`Set ${si + 1}`}>
                            <span className="set-circle__number">{si + 1}</span>
                            {done && <span className="set-circle__check">✓</span>}
                            <span className="set-circle__detail">{comp.payload.reps}×{comp.payload.weight_kg}kg</span>
                        </button>
                    ))}
                </div>
                <div className="exercise-card__stats">
                    <div className="mini-stat"><span className="mini-stat__value numeric">{comp.payload.sets ?? 3}</span><span className="mini-stat__label">Sets</span></div>
                    <div className="mini-stat"><span className="mini-stat__value numeric">{comp.payload.reps ?? 10}</span><span className="mini-stat__label">Reps</span></div>
                    <div className="mini-stat"><span className="mini-stat__value numeric">{comp.payload.weight_kg ?? 0}</span><span className="mini-stat__label">kg</span></div>
                    <div className="mini-stat mini-stat--rest"><span className="mini-stat__value numeric">{comp.payload.rest_seconds ?? 90}s</span><span className="mini-stat__label">Rest</span></div>
                </div>
                {isResting && (
                    <div className="rest-timer-live">
                        <div className="rest-timer-live__ring">
                            <svg viewBox="0 0 60 60">
                                <circle cx="30" cy="30" r="26" fill="none" stroke="rgba(255,255,255,0.08)" strokeWidth="4" />
                                <circle cx="30" cy="30" r="26" fill="none" stroke="var(--accent-secondary)" strokeWidth="4" strokeDasharray={`${2 * Math.PI * 26}`} strokeDashoffset={`${2 * Math.PI * 26 * (1 - restTimer.seconds / ((comp.payload.rest_seconds || 90) as number))}`} strokeLinecap="round" transform="rotate(-90 30 30)" style={{ transition: 'stroke-dashoffset 1s linear' }} />
                            </svg>
                            <span className="rest-timer-live__seconds numeric">{restTimer.seconds}</span>
                        </div>
                        <span className="rest-timer-live__label">Rest Timer</span>
                        <button className="rest-timer-live__skip" onClick={restTimer.stop}>Skip →</button>
                    </div>
                )}
                {exInfo && (<>
                    <button className="exercise-card__expand-btn" onClick={() => setExpandedCard(isExp ? null : idx)}>{isExp ? '▲ Hide Details' : '▼ Form Tips & Instructions'}</button>
                    {isExp && (
                        <div className="exercise-card__details">
                            {exInfo.secondaryMuscles.length > 0 && (<div className="exercise-card__secondary"><span className="exercise-card__detail-label">Also works:</span>{exInfo.secondaryMuscles.map((m, i) => <span key={i} className="exercise-card__secondary-tag">{m}</span>)}</div>)}
                            <div className="exercise-card__instructions">{exInfo.instructions.map((step, i) => <p key={i} className="exercise-card__step">{step}</p>)}</div>
                        </div>
                    )}
                </>)}
            </div>
        );
    };

    const handleSaveWorkout = async () => {
        if (!payload?.components) return;
        const exercises = payload.components.filter(c => c.type === 'set_tracker').map(c => ({
            name: c.payload.exercise_name || 'Exercise',
            sets: c.payload.sets || 3,
            reps: c.payload.reps || 10,
            weight: c.payload.weight_kg || 0,
        }));
        const { totalSets, completedSets } = getProgress();
        const workout = {
            id: Date.now().toString(),
            date: new Date().toISOString(),
            focus: focus || 'Full Body',
            exercises,
            completedSets,
            totalSets,
        };
        const saved = JSON.parse(localStorage.getItem('gymbrain_workouts') || '[]');
        saved.push(workout);
        localStorage.setItem('gymbrain_workouts', JSON.stringify(saved));

        try {
            const apiResult = await saveWorkout(JSON.stringify(payload));
            if (apiResult.error) {
                alert(`⚠️ Saved locally, but cloud sync failed: ${apiResult.error}`);
            } else {
                alert('✅ Workout saved to cloud and locally!');
            }
        } catch (e) {
            console.error(e);
            alert('✅ Workout saved locally (offline mode)');
        }
    };

    if (loading) return (<div className="app-content"><div className="m3-spinner-container"><div className="m3-spinner" /><p className="md-body-lg">Generating your workout with AI...</p><p className="md-body-sm text-muted">SafetyGate validating output</p></div></div>);
    const profile = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
    const profileContext = profile.name
        ? `${profile.level || 'Intermediate'} | ${profile.goal || 'muscle'} | ${profile.equipment?.join(', ') || 'Bodyweight'}`
        : null;

    if (!payload) return (
        <div className="app-content">
            <div style={{ textAlign: 'center', marginBottom: 20 }}>
                <div style={{ fontSize: '2.5rem', marginBottom: 4 }}>⚡</div>
                <h2 className="md-headline-sm" style={{ color: 'var(--md-primary)' }}>Ready to Train</h2>
                {profileContext && <p className="md-body-sm text-muted">{profileContext}</p>}
            </div>
            {profileContext && (
                <div className="m3-card mb-md" style={{ display: 'flex', justifyContent: 'space-around', textAlign: 'center' }}>
                    <div><div className="md-label-sm text-muted">Level</div><div className="md-body-md">{profile.level}</div></div>
                    <div><div className="md-label-sm text-muted">Goal</div><div className="md-body-md">{profile.goal}</div></div>
                    <div><div className="md-label-sm text-muted">Days</div><div className="md-body-md">{profile.daysPerWeek}x/wk</div></div>
                </div>
            )}
            <div className="m3-card">
                <div className="m3-field"><label className="m3-field__label" htmlFor="focus">Workout Focus</label>
                    <select id="focus" className="m3-select" value={focus} onChange={e => setFocus(e.target.value)}><option value="">Full Body</option><option value="upper body strength">Upper Body Strength</option><option value="lower body power">Lower Body Power</option><option value="chest and arms">Chest &amp; Arms</option><option value="back and shoulders">Back &amp; Shoulders</option><option value="core and abs">Core &amp; Abs</option></select>
                </div>
                {error && <div className="m3-error-banner">{error}</div>}
                <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" onClick={handleStart}>🚀 Generate Workout</button>
            </div>
            <div style={{ display: 'flex', gap: 8, marginTop: 16, flexWrap: 'wrap', justifyContent: 'center' }}>
                <span className="chip chip-success">✓ AI Ready</span>
                {profile.equipment?.length > 0 && <span className="chip chip-info">{profile.equipment.length} Equipment</span>}
                {profile.focusAreas?.length > 0 && <span className="chip chip-warning">{profile.focusAreas.join(', ')}</span>}
            </div>
        </div>
    );

    const { totalSets, completedSets, percent } = getProgress();
    const exercises = payload.components?.filter(c => c.type === 'set_tracker') || [];
    return (
        <div className="app-content">
            <div className="workout-progress-header">
                <div className="workout-progress-header__top"><h2 className="workout-progress-header__title">Your Workout</h2><span className="workout-progress-header__counter numeric">{completedSets}/{totalSets}</span></div>
                <p className="workout-progress-header__subtitle">AI-generated • SafetyGate validated</p>
                <div className="workout-progress-bar"><div className="workout-progress-bar__fill" style={{ width: `${percent}%` }} /></div>
                {percent === 100 && <div className="workout-complete-banner">🎉 Workout Complete! Great job!</div>}
            </div>
            {payload.components?.map((comp, idx) => {
                if (comp.type === 'tone_card') return (<div key={idx} className="tone-card" style={{ animationDelay: `${idx * 0.08}s` }}><div className="tone-card__emoji">💬</div><p>"{comp.payload.message}"</p>{comp.payload.persona && <span className="persona-tag">— {comp.payload.persona as string}</span>}</div>);
                if (comp.type === 'set_tracker') return renderExerciseCard(comp, idx);
                return null;
            })}
            {(!payload.components || payload.components.length === 0) && (<div className="m3-card m3-card--outlined" style={{ textAlign: 'center' }}><p className="text-muted">No workout components returned.</p></div>)}
            {exercises.length > 0 && (<div className="workout-summary-card"><div className="workout-summary-card__row">
                <div className="workout-summary-card__stat"><span className="numeric">{exercises.length}</span><span>Exercises</span></div>
                <div className="workout-summary-card__stat"><span className="numeric">{totalSets}</span><span>Total Sets</span></div>
                <div className="workout-summary-card__stat"><span className="numeric">{exercises.reduce((s, c) => s + ((c.payload.sets || 3) as number) * ((c.payload.reps || 10) as number), 0)}</span><span>Total Reps</span></div>
            </div></div>)}
            <div style={{ display: 'flex', gap: 8, marginTop: 16 }}>
                <button className="m3-btn m3-btn--tonal" style={{ flex: 1 }} onClick={handleSaveWorkout}>💾 Save</button>
                <button className="m3-btn m3-btn--outlined" style={{ flex: 1 }} onClick={() => { setPayload(null); setExerciseImages(new Map()); setSetProgress(new Map()); setExpandedCard(null); restTimer.stop(); }}>🔄 New</button>
            </div>
        </div>
    );
}
