import { useState, useEffect, useRef, useCallback } from 'react';
import { startWorkout, saveWorkout, getSubstitute, trackEvent } from '../services/api';
import type { SubstituteOption } from '../services/api';
import { searchExercise, type ExerciseDbItem } from '../services/exerciseDb';
import { useAuth } from '../context/auth';
import type { Completion } from '../services/workoutHistory';

// ─── Types ────────────────────────────────────────────────────────────────────
interface SduiPayload {
    message?: string; persona?: string; exercise_id?: string; exercise_name?: string;
    target_muscle?: string; sets?: number; reps?: number; weight_kg?: number;
    rest_seconds?: number; coach_tip?: string; phase?: string;
    [key: string]: string | number | boolean | undefined;
}
interface SduiComponent { type: string; payload: SduiPayload; swapped?: boolean; swappedFrom?: string; }
interface MegaPayload { screen_id?: string; components?: SduiComponent[]; }
interface SetProgress { completed: boolean[]; actualWeight: number[]; actualReps: number[]; }

// ─── Toast ────────────────────────────────────────────────────────────────────
interface Toast { id: number; message: string; variant: 'success' | 'error' | 'info'; }
function useToast() {
    const [toasts, setToasts] = useState<Toast[]>([]);
    const nextId = useRef(0);
    const show = useCallback((message: string, variant: Toast['variant'] = 'info') => {
        const id = ++nextId.current;
        setToasts(t => [...t, { id, message, variant }]);
        setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 3000);
    }, []);
    return { toasts, show };
}

// ─── Rest Timer ───────────────────────────────────────────────────────────────
function useRestTimer() {
    const [seconds, setSeconds] = useState(0);
    const [isRunning, setIsRunning] = useState(false);
    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const [duration, setDuration] = useState(0);
    const start = useCallback((duration: number) => {
        setDuration(duration);
        setSeconds(duration);
        setIsRunning(true);
    }, []);
    const stop = useCallback(() => { setIsRunning(false); setSeconds(0); }, []);
    useEffect(() => {
        if (isRunning && seconds > 0) {
            intervalRef.current = setInterval(() => {
                setSeconds(s => {
                    if (s <= 1) {
                        setIsRunning(false);
                        // Haptic feedback when timer hits 0
                        if (navigator.vibrate) navigator.vibrate([200, 100, 200, 100, 200]);
                        // Audio chime
                        try {
                            const AudioContextClass = window.AudioContext || (window as Window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
                            if (AudioContextClass) {
                                const ctx = new AudioContextClass();
                                const osc = ctx.createOscillator();
                                const gain = ctx.createGain();
                                osc.type = 'sine';
                                osc.frequency.setValueAtTime(880, ctx.currentTime); // A5
                                osc.frequency.exponentialRampToValueAtTime(440, ctx.currentTime + 0.3); // A4
                                gain.gain.setValueAtTime(0.3, ctx.currentTime);
                                gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.3);
                                osc.connect(gain);
                                gain.connect(ctx.destination);
                                osc.start();
                                osc.stop(ctx.currentTime + 0.3);
                            }
                        } catch { /* ignore if audio blocked */ }
                        return 0;
                    }
                    return s - 1;
                });
            }, 1000);
        }
        return () => { if (intervalRef.current) clearInterval(intervalRef.current); };
    }, [isRunning, seconds]);
    return { seconds, isRunning, start, stop, duration };
}

// ─── Substitute Modal ─────────────────────────────────────────────────────────
interface SubstituteModalProps {
    exerciseId: string;
    exerciseName: string;
    onSelect: (sub: SubstituteOption) => void;
    onDismiss: () => void;
}
function SubstituteModal({ exerciseId, exerciseName, onSelect, onDismiss }: SubstituteModalProps) {
    const [subs, setSubs] = useState<SubstituteOption[]>([]);
    const [loading, setLoading] = useState(true);
    const [offline, setOffline] = useState(false);

    useEffect(() => {
        let cancelled = false;
        const timeout = setTimeout(() => {
            if (!cancelled) {
                setOffline(true);
                setLoading(false);
                setSubs([]);
            }
        }, 3000);

        getSubstitute(exerciseId).then(res => {
            clearTimeout(timeout);
            if (cancelled) return;
            if (res.data?.substitutes && res.data.substitutes.length > 0) {
                setSubs(res.data.substitutes);
                setOffline(false);
            } else {
                setSubs([]);
                setOffline(true);
            }
            setLoading(false);
        }).catch(() => {
            clearTimeout(timeout);
            if (cancelled) return;
            setSubs([]);
            setOffline(true);
            setLoading(false);
        });
        return () => { cancelled = true; clearTimeout(timeout); };
    }, [exerciseId]);

    return (
        <div style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)',
            display: 'flex', alignItems: 'flex-end', justifyContent: 'center',
            zIndex: 1000, animation: 'fadeIn 0.2s ease'
        }}>
            <div style={{
                background: 'var(--surface-elevated, #1e2030)', borderRadius: '20px 20px 0 0',
                padding: '24px 20px 36px', width: '100%', maxWidth: 480,
                maxHeight: '80vh', overflowY: 'auto'
            }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                    <h3 style={{ color: 'var(--md-primary, #00FA9A)', margin: 0 }}>🔄 Swap Exercise</h3>
                    <button onClick={onDismiss} style={{ background: 'none', border: 'none', color: 'var(--md-on-surface, #fff)', fontSize: 22, cursor: 'pointer', padding: 4, lineHeight: 1 }}>✕</button>
                </div>
                <p style={{ color: 'var(--md-on-surface-variant, #aaa)', fontSize: 13, marginBottom: 16 }}>
                    Original: <strong style={{ color: '#fff' }}>{exerciseName}</strong>
                </p>
                {offline && <div style={{ background: 'rgba(255,165,0,0.15)', border: '1px solid #FFBF00', borderRadius: 8, padding: '6px 12px', fontSize: 12, color: '#FFBF00', marginBottom: 12 }}>Could not load alternatives. Check your connection and try again.</div>}
                {loading ? (
                    <div style={{ textAlign: 'center', padding: 40 }}><div className="m3-spinner" style={{ width: 32, height: 32, borderWidth: 3 }} /><p style={{ color: '#aaa', marginTop: 12, fontSize: 14 }}>Checking alternatives against your profile...</p></div>
                ) : subs.length === 0 ? (
                    <div style={{ textAlign: 'center', padding: 20, color: '#aaa' }}>
                        <p>No alternatives match your current profile.</p>
                        <p style={{ fontSize: 12 }}>Consider skipping this exercise.</p>
                        <button className="m3-btn m3-btn--outlined" style={{ marginTop: 12 }} onClick={onDismiss}>I'll Wait</button>
                    </div>
                ) : (
                    <>
                        {subs.map((sub, i) => (
                            <div key={i} style={{ background: 'rgba(255,255,255,0.06)', borderRadius: 12, padding: '14px 16px', marginBottom: 10, display: 'flex', flexDirection: 'column', gap: 8 }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                    <div>
                                        <div style={{ fontWeight: 600, color: '#fff', fontSize: 15 }}>{sub.name}</div>
                                        <div style={{ color: '#aaa', fontSize: 12, marginTop: 2 }}>🏋️ {sub.equipment}</div>
                                    </div>
                                    <button
                                        className="m3-btn m3-btn--filled"
                                        style={{ background: '#00FA9A', color: '#000', fontSize: 13, padding: '8px 14px', minHeight: 36, whiteSpace: 'nowrap' }}
                                        onClick={() => onSelect(sub)}
                                    >
                                        Use This
                                    </button>
                                </div>
                                <p style={{ color: '#bbb', fontSize: 12, margin: 0 }}>{sub.reason}</p>
                            </div>
                        ))}
                        <button className="m3-btn m3-btn--outlined" style={{ width: '100%', marginTop: 8 }} onClick={onDismiss}>I'll Wait</button>
                    </>
                )}
            </div>
        </div>
    );
}

// ─── Main Component ───────────────────────────────────────────────────────────
export default function WorkoutPage() {
    const { user } = useAuth();
    const draftKey = `gymbrain_active_workout:${user!.userId}`;
    const [sessionId, setSessionId] = useState(() => crypto.randomUUID());
    const [saving, setSaving] = useState(false);
    const [saved, setSaved] = useState(false);
    const [pendingJson, setPendingJson] = useState<string | null>(null);
    const saveInFlight = useRef(false);
    const [saveError, setSaveError] = useState('');
    const [focus, setFocus] = useState('');
    const [payload, setPayload] = useState<MegaPayload | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [exerciseImages, setExerciseImages] = useState<Map<string, ExerciseDbItem>>(new Map());
    const [setProgress, setSetProgress] = useState<Map<number, SetProgress>>(new Map());
    const [expandedCard, setExpandedCard] = useState<number | null>(null);
    const workoutView = useRef<HTMLDivElement>(null);
    const revealExercise = () => requestAnimationFrame(() => {
        workoutView.current?.scrollTo({ top: 0, behavior: 'instant' });
        workoutView.current?.querySelector<HTMLElement>('.exercise-name')?.focus({ preventScroll: true });
    });
    const [selectedExercise, setSelectedExercise] = useState<number | null>(null);
    const [showAllExercises, setShowAllExercises] = useState(false);
    const restTimer = useRestTimer();
    const [restExerciseIdx, setRestExerciseIdx] = useState<number | null>(null);
    const { toasts, show: showToast } = useToast();

    // Machine Taken state
    const [substituteModal, setSubstituteModal] = useState<{ idx: number; exerciseId: string; exerciseName: string } | null>(null);

    // Phase 3: sessionStorage persistence
    useEffect(() => {
        if (payload && !saved) {
            try {
                sessionStorage.setItem(draftKey, JSON.stringify({ payload, focus, sessionId, progress: [...setProgress], pendingJson }));
            } catch { /* ignore storage quota */ }
        }
    }, [payload, focus, sessionId, setProgress, pendingJson, draftKey, saved]);

    const [showResume, setShowResume] = useState(() => !!sessionStorage.getItem(draftKey));

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
        const names = payload.components
            .filter(c => c.type === 'set_tracker' && c.payload.exercise_name)
            .map(c => c.payload.exercise_name as string);
        if (names.length === 0) return;
        let cancelled = false;
        const fetchImages = async () => {
            const images = new Map<string, ExerciseDbItem>();
            for (const name of names) {
                const item = await searchExercise(name);
                if (item) images.set(name, item);
            }
            if (!cancelled) setExerciseImages(images);
        };
        void fetchImages();
        return () => { cancelled = true; };
    }, [payload]);

    const initialProgress = (workout: MegaPayload) => {
        const progress = new Map<number, SetProgress>();
        workout.components?.forEach((comp, idx) => {
            if (comp.type === 'set_tracker') {
                const n = comp.payload.sets || 3;
                progress.set(idx, { completed: Array(n).fill(false), actualWeight: Array(n).fill(comp.payload.weight_kg || 0), actualReps: Array(n).fill(comp.payload.reps || 10) });
            }
        });
        return progress;
    };

    const handleStart = async () => {
        setError(''); setLoading(true); setShowResume(false);
        const result = await startWorkout(focus || undefined);
        setLoading(false);
        if (result.error) { setError(result.error); return; }
        if (result.data?.megaPayloadJson) {
            try {
                const parsed = parseMegaPayload(result.data.megaPayloadJson);
                setSessionId(crypto.randomUUID()); setSaved(false); setPendingJson(null); setSaveError('');
                setSetProgress(initialProgress(parsed));
                setPayload(parsed);
                const exerciseCount = parsed.components?.filter(c => c.type === 'set_tracker').length || 0;
                trackEvent('workout_started', { exerciseCount });
            } catch (e) { setError(`Parse error: ${e instanceof Error ? e.message : 'Unknown'}`); }
        }
    };

    const handleResume = () => {
        try {
            const saved = sessionStorage.getItem(draftKey);
            if (saved) { const draft = JSON.parse(saved); setSessionId(draft.sessionId || crypto.randomUUID()); setPendingJson(draft.pendingJson || null); setSetProgress(draft.progress ? new Map(draft.progress) : initialProgress(draft.payload)); setPayload(draft.payload); setFocus(draft.focus || ''); }
        } catch { /* ignore */ }
        setShowResume(false);
    };

    const toggleSet = (ei: number, si: number) => {
        const current = setProgress.get(ei);
        if (current && !current.completed[si] && current.completed.every((done, index) => done || index === si)) {
            setSelectedExercise(null);
            revealExercise();
        }
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

    // Machine Taken: handle exercise swap
    const handleSubstituteSelect = (idx: number, sub: SubstituteOption) => {
        setPayload(prev => {
            if (!prev?.components) return prev;
            const comps = [...prev.components];
            const original = comps[idx];
            comps[idx] = {
                ...original,
                swapped: true,
                swappedFrom: original.payload.exercise_id,
                payload: {
                    ...original.payload,
                    exercise_id: sub.exerciseId,
                    exercise_name: sub.name,
                    // Preserve all other fields (sets, reps, weight, etc.)
                }
            };
            return { ...prev, components: comps };
        });
        trackEvent('exercise_substituted', { from: substituteModal?.exerciseId, to: sub.exerciseId });
        showToast(`✅ Swapped to ${sub.name}`, 'success');
        setSubstituteModal(null);
    };

    const renderExerciseCard = (comp: SduiComponent, idx: number) => {
        const progress = setProgress.get(idx);
        const exInfo = exerciseImages.get(comp.payload.exercise_name as string);
        const isExp = expandedCard === idx;
        const doneSets = progress?.completed.filter(Boolean).length || 0;
        const totalS = progress?.completed.length || (comp.payload.sets as number) || 3;
        const allDone = doneSets === totalS;

        return (
            <section key={idx} className={`exercise-card workout-current ${allDone ? 'exercise-card--done' : ''}`} aria-label={`Current exercise: ${comp.payload.exercise_name || 'Exercise'}`}>
                <div className="exercise-card__progress-bar"><div className="exercise-card__progress-fill" style={{ width: `${(doneSets / totalS) * 100}%` }} /></div>
                <p className="workout-current__eyebrow">{allDone ? 'Exercise complete' : `Now training · ${doneSets} of ${totalS} sets done`}{comp.swapped ? ' · Alternative' : ''}</p>
                <div className="exercise-card__header">
                    {exInfo?.gifUrl ? (
                        <div className="exercise-card__gif-container"><img src={exInfo.gifUrl} alt={comp.payload.exercise_name as string} className="exercise-card__gif" loading="lazy" /></div>
                    ) : (
                        <div className="exercise-card__gif-placeholder" aria-hidden="true">🏋️</div>
                    )}
                    <div className="exercise-card__title-block">
                        <h3 className="exercise-name" tabIndex={-1}>{comp.payload.exercise_name || 'Exercise'}</h3>
                        <div className="exercise-card__meta">
                            {comp.payload.target_muscle && <span className="exercise-card__muscle-tag">{comp.payload.target_muscle as string}</span>}
                            {exInfo?.equipment && <span className="exercise-card__equipment-tag">{exInfo.equipment}</span>}
                        </div>
                    </div>
                </div>
                <p className="workout-current__prescription">{totalS} sets · {comp.payload.reps ?? 10} reps · {comp.payload.weight_kg ?? 0} kg · Rest {comp.payload.rest_seconds ?? 90}s</p>
                {progress && <div className="workout-set-list">
                    {progress.completed.map((done, si) => <div className="workout-set" key={si}>
                        <div className="workout-set__fields">
                            <span className="workout-set__name">Set {si + 1}</span>
                            <label>Reps<input aria-label={`${comp.payload.exercise_name} set ${si + 1} reps`} className="m3-input" type="number" min={0} max={1000} step={1} value={progress.actualReps[si]}
                                onChange={e => setSetProgress(prev => { const next = new Map(prev); const p = next.get(idx)!; const reps = [...p.actualReps]; reps[si] = Math.max(0, Math.min(1000, Math.trunc(Number(e.target.value)))); next.set(idx, { ...p, actualReps: reps }); return next; })} /></label>
                            <label>kg<input aria-label={`${comp.payload.exercise_name} set ${si + 1} weight kg`} className="m3-input" type="number" min={0} max={1000} step={0.5} value={progress.actualWeight[si]}
                                onChange={e => setSetProgress(prev => { const next = new Map(prev); const p = next.get(idx)!; const weights = [...p.actualWeight]; weights[si] = Math.max(0, Math.min(1000, Number(e.target.value))); next.set(idx, { ...p, actualWeight: weights }); return next; })} /></label>
                        </div>
                        <button className={`set-circle ${done ? 'set-circle--done' : ''}`} title={`Set ${si + 1}`} aria-label={`${done ? 'Undo' : 'Complete'} set ${si + 1} of ${comp.payload.exercise_name || 'Exercise'}`} aria-pressed={done} onClick={() => toggleSet(idx, si)}>
                            {done ? '✓ Set complete' : `Complete set ${si + 1}`}
                        </button>
                    </div>)}
                </div>}
                {!progress?.completed.some(Boolean) && (
                    <button className="workout-current__swap"
                        onClick={() => setSubstituteModal({
                            idx,
                            exerciseId: comp.payload.exercise_id as string || '',
                            exerciseName: comp.payload.exercise_name as string || 'Exercise'
                        })}
                    >
                        Equipment busy? Find an alternative
                    </button>
                )}
                {exInfo && (<>
                    <button className="exercise-card__expand-btn" aria-expanded={isExp} onClick={() => setExpandedCard(isExp ? null : idx)}>{isExp ? 'Hide form tips' : 'Show form tips'}</button>
                    {isExp && (
                        <div className="exercise-card__details">
                            {exInfo.secondaryMuscles.length > 0 && (<div className="exercise-card__secondary"><span className="exercise-card__detail-label">Also works:</span>{exInfo.secondaryMuscles.map((m, i) => <span key={i} className="exercise-card__secondary-tag">{m}</span>)}</div>)}
                            <div className="exercise-card__instructions">{exInfo.instructions.map((step, i) => <p key={i} className="exercise-card__step">{step}</p>)}</div>
                        </div>
                    )}
                </>)}
            </section>
        );
    };

    const handleSaveWorkout = async () => {
        if (!payload?.components || saveInFlight.current || saved) return false;
        if (getProgress().completedSets === 0) { setSaveError('Record at least one completed set before saving.'); return false; }
        const completion: Completion = { schemaVersion: 1, focus: focus || 'Full Body',
            exercises: payload.components.flatMap((comp, index) => {
                const progress = setProgress.get(index);
                if (comp.type !== 'set_tracker' || !progress) return [];
                return [{ exerciseId: comp.payload.exercise_id, name: comp.payload.exercise_name || 'Exercise',
                    sets: progress.completed.map((completed, i) => ({ completed, reps: progress.actualReps[i], weightKg: progress.actualWeight[i] })) }];
            }),
        };
        const json = pendingJson || JSON.stringify(completion);
        setPendingJson(json);
        // Freeze and persist the exact first attempt before sending, for lost-response retries.
        try { sessionStorage.setItem(draftKey, JSON.stringify({ payload, focus, sessionId, progress: [...setProgress], pendingJson: json })); } catch { /* request remains retryable while mounted */ }
        saveInFlight.current = true; setSaving(true); setSaveError('');
        const result = await saveWorkout(json, sessionId);
        saveInFlight.current = false; setSaving(false);
        if (result.error || !result.data) { setSaveError(result.error || 'Save failed. Retry this session.'); return false; }
        setSaved(true); sessionStorage.removeItem(draftKey); restTimer.stop();
        trackEvent('workout_completed', { exerciseCount: completion.exercises.length, completedSets: getProgress().completedSets });
        showToast('Workout saved to your account.', 'success');
        return true;
    };

    const resetWorkout = () => {
        sessionStorage.removeItem(draftKey); setPayload(null); setExerciseImages(new Map());
        setSetProgress(new Map()); setExpandedCard(null); setSelectedExercise(null); setShowAllExercises(false); restTimer.stop(); setSaved(false); setPendingJson(null); setSaveError('');
    };

    // ─── Render ───────────────────────────────────────────────────────────────
    if (loading) return (<div className="app-content"><div className="m3-spinner-container"><div className="m3-spinner" /><p className="md-body-lg">Generating your workout with AI...</p><p className="md-body-sm text-muted">Checking the generated workout</p></div></div>);

    const profile = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
    const profileContext = profile.name
        ? `${profile.level || 'Intermediate'} | ${profile.goal || 'muscle'} | ${profile.equipment?.join(', ') || 'Bodyweight'}`
        : null;

    if (showResume && !payload) return (
        <div className="app-content" style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '60vh', gap: 16 }}>
            <div style={{ fontSize: '3rem' }}>💪</div>
            <h2 className="md-headline-sm" style={{ color: 'var(--md-primary)', textAlign: 'center' }}>Resume Your Workout?</h2>
            <p className="md-body-sm text-muted" style={{ textAlign: 'center' }}>This unfinished workout is saved only in this browser tab. Closing the tab may lose it.</p>
            <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" style={{ maxWidth: 320 }} onClick={handleResume}>▶ Resume Workout</button>
            <button className="m3-btn m3-btn--outlined" style={{ maxWidth: 320 }} onClick={() => { sessionStorage.removeItem(draftKey); setShowResume(false); }}>Start Fresh</button>
        </div>
    );

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
    const exerciseEntries = (payload.components || []).map((comp, idx) => ({ comp, idx })).filter(({ comp }) => comp.type === 'set_tracker');
    const exercises = exerciseEntries.map(({ comp }) => comp);
    const warmups = (payload.components || []).filter(comp => comp.type === 'warmup_card');
    const nextExercise = exerciseEntries.find(({ idx }) => !setProgress.get(idx)?.completed.every(Boolean))?.idx;
    const activeExercise = selectedExercise ?? nextExercise ?? exerciseEntries[0]?.idx;
    const queuedExercises = exerciseEntries.filter(({ idx }) => idx !== activeExercise);
    const visibleQueue = showAllExercises ? queuedExercises : queuedExercises.slice(0, 4);
    return (
        <div className="app-content workout-page" ref={workoutView}>
            {/* Toast notifications */}
            <div style={{ position: 'fixed', bottom: 80, left: '50%', transform: 'translateX(-50%)', zIndex: 2000, display: 'flex', flexDirection: 'column', gap: 8, alignItems: 'center', pointerEvents: 'none' }}>
                {toasts.map(t => (
                    <div key={t.id} style={{
                        background: t.variant === 'success' ? '#00FA9A' : t.variant === 'error' ? '#FF4444' : '#3a8ef6',
                        color: t.variant === 'success' ? '#000' : '#fff',
                        borderRadius: 12, padding: '10px 20px', fontSize: 14, fontWeight: 600,
                        boxShadow: '0 4px 20px rgba(0,0,0,0.4)',
                        animation: 'slideUp 0.3s ease',
                        maxWidth: 320, textAlign: 'center'
                    }}>
                        {t.message}
                    </div>
                ))}
            </div>

            <div className="workout-progress-header">
                <div className="workout-progress-header__top"><h2 className="workout-progress-header__title">Today's workout</h2><span className="workout-progress-header__counter numeric">{completedSets}/{totalSets} sets</span></div>
                <p className="workout-progress-header__subtitle">{exercises.length} exercises · Pick any exercise below when you're ready</p>
                <div className="workout-progress-bar"><div className="workout-progress-bar__fill" style={{ width: `${percent}%` }} /></div>
                {percent === 100 && <div className="workout-complete-banner">🎉 Workout Complete! Great job!</div>}
            </div>

            {warmups.length > 0 && <details className="workout-warmup">
                <summary>Warm up first <span>{warmups.length} movements</span></summary>
                <ol>{warmups.map((comp, idx) => <li key={idx}><strong>{comp.payload.exercise_name || 'Warm-up'}</strong><span>{comp.payload.notes || 'Move gently and prepare for your workout.'}</span></li>)}</ol>
            </details>}
            {restTimer.isRunning && <div className="workout-rest" role="timer" aria-label="Rest timer">
                <span>Resting after {payload.components?.[restExerciseIdx ?? -1]?.payload.exercise_name || 'your set'}</span>
                <strong>{Math.floor(restTimer.seconds / 60)}:{String(restTimer.seconds % 60).padStart(2, '0')}</strong>
                <button type="button" onClick={restTimer.stop}>Skip rest</button>
            </div>}
            <fieldset disabled={saving || saved || !!pendingJson} style={{ border: 0, padding: 0, margin: 0, minWidth: 0 }}>
                {activeExercise !== undefined && renderExerciseCard(payload.components![activeExercise], activeExercise)}
            </fieldset>
            {payload.components?.filter(comp => comp.type === 'tone_card' && comp.payload.message).slice(0, 1).map((comp, idx) =>
                <p className="workout-coach-note" key={idx}>{comp.payload.message}</p>)}
            {queuedExercises.length > 0 && <section className="workout-queue" aria-label="Exercise list">
                <div className="workout-queue__heading"><h3>Exercises</h3><span>{exercises.length} total</span></div>
                <div className="workout-queue__list">{visibleQueue.map(({ comp, idx }) => {
                    const progress = setProgress.get(idx);
                    const completed = progress?.completed.filter(Boolean).length || 0;
                    const total = progress?.completed.length || comp.payload.sets || 3;
                    return <button type="button" className="workout-queue__item" key={idx} onClick={() => { setSelectedExercise(idx); setExpandedCard(null); revealExercise(); }}>
                        <span><strong>{comp.payload.exercise_name || 'Exercise'}</strong><small>{completed === total ? 'Complete' : `${completed}/${total} sets · ${comp.payload.target_muscle || 'Strength'}`}</small></span>
                        <span aria-hidden="true">›</span>
                    </button>;
                })}</div>
                {queuedExercises.length > 4 && <button type="button" className="workout-queue__more" onClick={() => setShowAllExercises(v => !v)}>{showAllExercises ? 'Show fewer exercises' : `Show all ${exercises.length} exercises`}</button>}
            </section>}
            {(!payload.components || payload.components.length === 0) && (<div className="m3-card m3-card--outlined" style={{ textAlign: 'center' }}><p className="text-muted">No workout components returned.</p></div>)}
            {exercises.length > 0 && (<div className="workout-summary-card"><div className="workout-summary-card__row">
                <div className="workout-summary-card__stat"><span className="numeric">{exercises.length}</span><span>Exercises</span></div>
                <div className="workout-summary-card__stat"><span className="numeric">{totalSets}</span><span>Total Sets</span></div>
                <div className="workout-summary-card__stat"><span className="numeric">{exercises.reduce((s, c) => s + ((c.payload.sets || 3) as number) * ((c.payload.reps || 10) as number), 0)}</span><span>Total Reps</span></div>
            </div></div>)}
            {!saved && <p className="md-body-sm text-muted">Unfinished workouts stay in this browser tab and may be lost when it closes. Connect to the internet and choose Finish &amp; Save to keep this workout in your account.</p>}

            {saveError && <div role="alert" className="m3-error-banner">Not confirmed saved: {saveError} Your session is retained in this browser tab. Retry to confirm it.</div>}
            {saved && <p role="status">Saved to your account. View it in History.</p>}
            <div className="workout-actions">
                <button className="m3-btn m3-btn--filled" disabled={saving || saved || completedSets === 0} onClick={handleSaveWorkout}>
                    {saving ? 'Saving…' : saved ? 'Saved' : pendingJson ? 'Retry save' : 'Finish & Save'}
                </button>
                <button className="m3-btn m3-btn--outlined" disabled={saving || (!!pendingJson && !saved)} onClick={async () => {
                    if (!saved && completedSets > 0) {
                        if (!window.confirm('Finish and save this session before starting another?')) return;
                        if (!await handleSaveWorkout()) return;
                    }
                    resetWorkout();
                }}>New workout</button>
            </div>

            {/* Substitute Modal */}
            {substituteModal && (
                <SubstituteModal
                    exerciseId={substituteModal.exerciseId}
                    exerciseName={substituteModal.exerciseName}
                    onSelect={(sub) => handleSubstituteSelect(substituteModal.idx, sub)}
                    onDismiss={() => setSubstituteModal(null)}
                />
            )}
        </div>
    );
}
