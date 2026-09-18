import { useState } from 'react';
import { login, register, connectFirebase } from '../services/api';
import { useAuth } from '../context/auth';
import { firebaseAuth, firebaseEnabled, googleSignIn, emailSignIn, verifyEmail, resetPassword, authError } from '../services/firebase';

export default function AuthPage() {
    const { setAuth } = useAuth();
    const [isLogin, setIsLogin] = useState(true);
    const [legacy, setLegacy] = useState(false);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [legacyPassword, setLegacyPassword] = useState('');
    const [needsLink, setNeedsLink] = useState(false);
    const [needsVerification, setNeedsVerification] = useState(false);
    const [error, setError] = useState('');
    const [notice, setNotice] = useState('');
    const [loading, setLoading] = useState(false);

    async function finishFirebase() {
        const user = firebaseAuth?.currentUser;
        if (!user) throw new Error('Please sign in again.');
        await user.reload();
        if (!user.emailVerified) {
            setNeedsVerification(true);
            setNotice('Verify your email before continuing. Use Send verification email, then open the link in your inbox.');
            return;
        }
        const token = await user.getIdToken(true);
        const result = await connectFirebase(token, needsLink ? legacyPassword : undefined);
        if (result.code === 'account_link_required') { setNeedsLink(true); if (needsLink) setError('The account could not be linked. Check your previous GymBrain password.'); else setNotice('An existing GymBrain account needs to be linked.'); return; }
        if (result.error) throw new Error(result.error);
        if (result.data) {
            localStorage.setItem('gymbrain_auth_source', 'firebase');
            setAuth(result.data.userId, result.data.email, token);
        }
    }
    async function run(action: () => Promise<void>) {
        if (loading) return;
        setLoading(true); setError(''); setNotice('');
        try { await action(); } catch (err) { setError(authError(err)); }
        finally { setLoading(false); }
    }
    async function submit(e: React.FormEvent) {
        e.preventDefault();
        await run(async () => {
            if (needsLink || needsVerification) { await finishFirebase(); return; }
            if (firebaseEnabled && !legacy) {
                await emailSignIn(email, password, !isLogin);
                await finishFirebase();
                return;
            }
            const result = isLogin ? await login(email.trim(), password) : await register(email.trim(), password);
            if (result.error) throw new Error(result.error);
            if (result.data) {
                localStorage.removeItem('gymbrain_auth_source');
                setAuth(result.data.userId, email.trim(), result.data.token);
            }
        });
    }
    function resetForm() { setNeedsLink(false); setNeedsVerification(false); setLegacyPassword(''); setPassword(''); setError(''); setNotice(''); }

    return <div className="auth-page">
        <div className="auth-page__logo">🧠</div><h1 className="auth-page__title">GymBrain</h1>
        <p className="auth-page__subtitle">AI-Powered Fitness Coaching</p>
        <form className="auth-form" onSubmit={submit}>
            <h2 className="md-title-lg text-center mb-md">{needsLink ? 'Link your existing account' : needsVerification ? 'Check your email' : isLogin ? 'Welcome Back' : 'Create Account'}</h2>
            {firebaseEnabled && !needsLink && !needsVerification && <button type="button" className="m3-btn m3-btn--outlined m3-btn--full m3-btn--lg" disabled={loading}
                onClick={() => void run(async () => { await googleSignIn(); await finishFirebase(); })}>Sign in with Google</button>}
            {!needsLink && !needsVerification && <>
                <div className="m3-field"><label className="m3-field__label" htmlFor="email">Email</label>
                    <input id="email" className="m3-input" type="email" value={email} onChange={e => setEmail(e.target.value)} required autoComplete="email" /></div>
                <div className="m3-field"><label className="m3-field__label" htmlFor="password">Password</label>
                    <input id="password" className="m3-input" type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={isLogin ? undefined : 8} maxLength={256} autoComplete={isLogin ? 'current-password' : 'new-password'} /></div>
            </>}
            {needsLink && <>
                <p>Enter your previous GymBrain password to keep your workouts and profile. After linking, use Google or your Firebase email sign-in. Previous GymBrain sessions will end.</p>
                <div className="m3-field"><label className="m3-field__label" htmlFor="legacy-password">Previous GymBrain password</label>
                    <input id="legacy-password" className="m3-input" type="password" autoComplete="current-password" required maxLength={256} value={legacyPassword} onChange={e => setLegacyPassword(e.target.value)} /></div>
                <p>If you have forgotten this password, your old account needs assisted recovery. Firebase password reset cannot recover an account that has not been linked.</p>
            </>}
            {error && <div role="alert" className="m3-error-banner">{error}</div>}
            {notice && <p role="status">{notice}</p>}
            <button type="submit" className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" disabled={loading}>
                {loading ? 'Processing...' : needsLink ? 'Link account and continue' : needsVerification ? 'I verified my email — continue' : isLogin ? 'Sign In' : 'Create Account'}</button>
            {needsVerification && <button type="button" disabled={loading} onClick={() => void run(async () => { await verifyEmail(); setNotice('Verification email sent. Check your inbox and spam folder.'); })}>Send verification email</button>}
            {firebaseEnabled && !needsLink && !needsVerification && !legacy && <button type="button" disabled={loading} onClick={() => void run(async () => {
                if (!email.trim()) throw new Error('Enter your email address first.');
                try { await resetPassword(email); } catch (err) {
                    if (!(err && typeof err === 'object' && 'code' in err && err.code === 'auth/user-not-found')) throw err;
                }
                setNotice('If this email has a Firebase password account, a reset email will arrive shortly. Older, unlinked GymBrain accounts need assisted recovery.');
            })}>Forgot password?</button>}
            {!needsLink && !needsVerification && !legacy && <div className="auth-toggle"><button type="button" disabled={loading} onClick={() => { resetForm(); setIsLogin(!isLogin); }}>{isLogin ? 'Create an account' : 'Back to sign in'}</button></div>}
            {firebaseEnabled && <div className="auth-toggle"><button type="button" disabled={loading} onClick={() => { resetForm(); setIsLogin(true); setLegacy(!legacy); }}>{legacy || needsLink || needsVerification ? 'Back to sign in' : 'Previous account sign-in'}</button></div>}
            {legacy && <p>For accounts created before Firebase sign-in. To link one, sign in with Google using the same email, or create and verify a Firebase email account with that email.</p>}
        </form>
    </div>;
}
