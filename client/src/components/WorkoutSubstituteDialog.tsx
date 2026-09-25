import { useEffect, useId, useRef, useState } from 'react';
import { getSubstitute, type SubstituteOption } from '../services/api';
import './WorkoutSubstituteDialog.css';

interface WorkoutSubstituteDialogProps {
    exerciseId: string;
    exerciseName: string;
    onSelect: (sub: SubstituteOption) => void;
    onDismiss: () => void;
}

interface SubstituteResult {
    exerciseId: string;
    attempt: number;
    options: SubstituteOption[];
    failed: boolean;
}

export default function WorkoutSubstituteDialog({ exerciseId, exerciseName, onSelect, onDismiss }: WorkoutSubstituteDialogProps) {
    const dialogRef = useRef<HTMLDialogElement>(null);
    const headingRef = useRef<HTMLHeadingElement>(null);
    const titleId = useId();
    const descriptionId = useId();
    const [attempt, setAttempt] = useState(0);
    const [result, setResult] = useState<SubstituteResult | null>(null);
    const currentResult = result?.exerciseId === exerciseId && result.attempt === attempt ? result : null;

    useEffect(() => {
        const dialog = dialogRef.current;
        if (!dialog) return;
        const previousFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null;
        dialog.showModal();
        headingRef.current?.focus({ preventScroll: true });

        return () => {
            // Do not dismiss from a close event: StrictMode also runs this cleanup.
            if (dialog.open) dialog.close();
            if (previousFocus?.isConnected) previousFocus.focus({ preventScroll: true });
        };
    }, []);

    useEffect(() => {
        let cancelled = false;
        getSubstitute(exerciseId).then(response => {
            if (cancelled) return;
            const failed = Boolean(response.error) || !Array.isArray(response.data?.substitutes);
            setResult({ exerciseId, attempt, options: failed ? [] : response.data!.substitutes, failed });
        }).catch(() => {
            if (!cancelled) setResult({ exerciseId, attempt, options: [], failed: true });
        });
        return () => { cancelled = true; };
    }, [exerciseId, attempt]);

    return (
        <dialog
            ref={dialogRef}
            className="workout-substitute"
            aria-labelledby={titleId}
            aria-describedby={descriptionId}
            onCancel={event => { event.preventDefault(); onDismiss(); }}
            onKeyDown={event => {
                if (event.key !== 'Tab') return;
                const buttons = Array.from(event.currentTarget.querySelectorAll<HTMLButtonElement>('button:not(:disabled)'))
                    .filter(button => button.getClientRects().length > 0);
                const first = buttons[0];
                const last = buttons[buttons.length - 1];
                if (!first || !last) return;
                const active = document.activeElement;
                if (event.shiftKey && (active === first || active === headingRef.current || active === event.currentTarget)) {
                    event.preventDefault();
                    last.focus();
                } else if (!event.shiftKey && active === last) {
                    event.preventDefault();
                    first.focus();
                }
            }}
        >
            <header className="workout-substitute__header">
                <div>
                    <p className="workout-substitute__eyebrow">Make room for your workout</p>
                    <h2 ref={headingRef} id={titleId} tabIndex={-1}>Swap exercise</h2>
                </div>
                <button type="button" className="workout-substitute__close" aria-label="Close exercise alternatives" onClick={onDismiss}>
                    <span aria-hidden="true">×</span>
                </button>
            </header>

            <p id={descriptionId} className="workout-substitute__description">
                Choose an alternative to <strong>{exerciseName}</strong>, or keep your original exercise.
            </p>

            <div className="workout-substitute__content" aria-busy={!currentResult}>
                {!currentResult ? (
                    <div className="workout-substitute__state" role="status">
                        <span className="m3-spinner workout-substitute__spinner" aria-hidden="true" />
                        <p>Checking alternatives against your profile…</p>
                    </div>
                ) : currentResult.failed ? (
                    <div className="workout-substitute__state workout-substitute__state--error">
                        <div role="alert">
                            <h3>Could not load alternatives</h3>
                            <p>Please try again. Your original exercise is still available.</p>
                        </div>
                        <button type="button" className="workout-substitute__button workout-substitute__button--primary" onClick={() => setAttempt(value => value + 1)}>Try again</button>
                    </div>
                ) : currentResult.options.length === 0 ? (
                    <div className="workout-substitute__state" role="status">
                        <h3>No matching alternatives</h3>
                        <p>No alternatives match your current profile. You can keep the original exercise and return to your workout.</p>
                    </div>
                ) : (
                    <ul className="workout-substitute__options" aria-label="Alternative exercises">
                        {currentResult.options.map((sub, index) => (
                            <li key={`${sub.exerciseId}-${index}`} className="workout-substitute__option">
                                <div className="workout-substitute__option-heading">
                                    <div>
                                        <h3 id={`${titleId}-option-${index}`}>{sub.name}</h3>
                                        <p className="workout-substitute__equipment">{sub.equipment}</p>
                                    </div>
                                    <button
                                        type="button"
                                        className="workout-substitute__button workout-substitute__button--primary"
                                        aria-describedby={`${titleId}-option-${index}`}
                                        onClick={() => onSelect(sub)}
                                    >Use This</button>
                                </div>
                                <p className="workout-substitute__reason">{sub.reason}</p>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            <footer className="workout-substitute__footer">
                <button type="button" className="workout-substitute__button workout-substitute__button--keep" onClick={onDismiss}>Keep original exercise</button>
            </footer>
        </dialog>
    );
}
