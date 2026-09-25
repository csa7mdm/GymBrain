import { useCallback, useEffect, useState } from 'react';

export interface RestState {
    phase: 'idle' | 'running' | 'paused' | 'finished';
    deadline: number;
    remainingMs: number;
    exerciseIndex: number;
    setIndex: number;
}
const idle: RestState = { phase: 'idle', deadline: 0, remainingMs: 0, exerciseIndex: -1, setIndex: -1 };

export function useRestTimer() {
    const [state, setState] = useState<RestState>(idle);
    const [now, setNow] = useState(() => Date.now());
    useEffect(() => {
        if (state.phase !== 'running') return;
        const update = () => {
            const time = Date.now();
            setNow(time);
            if (time >= state.deadline) setState(s => s.phase === 'running' && s.deadline <= time
                ? { ...s, phase: 'finished', remainingMs: 0 } : s);
        };
        // Deadlines recover elapsed time after a suspended/background tab; ticks are display only.
        const interval = window.setInterval(update, 250);
        document.addEventListener('visibilitychange', update);
        window.addEventListener('focus', update);
        return () => {
            clearInterval(interval);
            document.removeEventListener('visibilitychange', update);
            window.removeEventListener('focus', update);
        };
    }, [state.phase, state.deadline]);

    const start = useCallback((seconds: number, exerciseIndex: number, setIndex: number) => {
        const time = Date.now();
        const remainingMs = Math.max(0, seconds) * 1000;
        setNow(time);
        setState({ phase: remainingMs > 0 ? 'running' : 'idle', deadline: time + remainingMs, remainingMs, exerciseIndex, setIndex });
    }, []);
    const stop = useCallback(() => setState(idle), []);
    const pause = () => setState(s => {
        if (s.phase !== 'running') return s;
        const remainingMs = Math.max(0, s.deadline - Date.now());
        return { ...s, phase: remainingMs ? 'paused' : 'finished', remainingMs };
    });
    const resume = () => {
        const time = Date.now();
        setNow(time);
        setState(s => s.phase === 'paused' ? { ...s, phase: 'running', deadline: time + s.remainingMs } : s);
    };
    const extend = () => {
        const time = Date.now();
        setNow(time);
        setState(s => s.phase === 'idle' ? s : s.phase === 'paused'
            ? { ...s, remainingMs: s.remainingMs + 15000 }
            : { ...s, phase: 'running', deadline: Math.max(time, s.deadline) + 15000 });
    };
    const restore = (value: unknown) => {
        if (!value || typeof value !== 'object') return;
        const s = value as RestState;
        if (!['idle', 'running', 'paused', 'finished'].includes(s.phase) ||
            ![s.deadline, s.remainingMs, s.exerciseIndex, s.setIndex].every(Number.isFinite) || s.remainingMs < 0) return;
        const time = Date.now();
        setNow(time);
        setState(s.phase === 'running' && s.deadline <= time ? { ...s, phase: 'finished', remainingMs: 0 } : s);
    };
    const seconds = Math.ceil((state.phase === 'running' ? Math.max(0, state.deadline - now)
        : state.phase === 'paused' ? state.remainingMs : 0) / 1000);
    return { state, seconds, start, stop, pause, resume, extend, restore };
}
