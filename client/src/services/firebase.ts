import { initializeApp } from 'firebase/app';
import { getAuth, GoogleAuthProvider, signInWithPopup, signInWithEmailAndPassword,
    createUserWithEmailAndPassword, sendEmailVerification, sendPasswordResetEmail, signOut } from 'firebase/auth';

// Public browser configuration; this is not a service-account credential.
const apiKey = import.meta.env.VITE_FIREBASE_API_KEY || (import.meta.env.PROD ? 'AIzaSyAGzUuIPkA8jVnyI7mwqYOiLkpVRo-29ug' : '');
export const firebaseEnabled = !!apiKey;
export const firebaseAuth = firebaseEnabled ? getAuth(initializeApp({
    apiKey,
    authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN || 'gymbrain-pilot-cairo.firebaseapp.com',
    projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID || 'gymbrain-pilot-cairo',
})) : null;

export async function googleSignIn() {
    if (!firebaseAuth) throw new Error('Google sign-in is not configured.');
    const provider = new GoogleAuthProvider();
    provider.setCustomParameters({ prompt: 'select_account' });
    return (await signInWithPopup(firebaseAuth, provider)).user;
}
export async function emailSignIn(email: string, password: string, create: boolean) {
    if (!firebaseAuth) throw new Error('Email sign-in is not configured.');
    return (await (create ? createUserWithEmailAndPassword : signInWithEmailAndPassword)(firebaseAuth, email.trim(), password)).user;
}
export async function verifyEmail() {
    if (!firebaseAuth?.currentUser) throw new Error('Sign in first.');
    await sendEmailVerification(firebaseAuth.currentUser);
}
export async function resetPassword(email: string) {
    if (!firebaseAuth) throw new Error('Password recovery is not configured.');
    await sendPasswordResetEmail(firebaseAuth, email.trim());
}
export async function firebaseToken() {
    if (!firebaseAuth) throw new Error('Sign-in is not configured.');
    await firebaseAuth.authStateReady();
    if (!firebaseAuth.currentUser) throw new Error('Your session expired. Please sign in again.');
    return firebaseAuth.currentUser.getIdToken();
}
export async function firebaseSignOut() { if (firebaseAuth) await signOut(firebaseAuth); }
export function authError(error: unknown) {
    const code = error && typeof error === 'object' && 'code' in error ? String(error.code) : '';
    const messages: Record<string, string> = {
        'auth/popup-closed-by-user': 'Google sign-in was cancelled. Please try again.',
        'auth/cancelled-popup-request': 'Google sign-in was cancelled. Please try again.',
        'auth/popup-blocked': 'Allow popups for GymBrain and try Google sign-in again.',
        'auth/unauthorized-domain': 'Google sign-in is not enabled for this website yet.',
        'auth/account-exists-with-different-credential': 'Sign in using your existing email and password first.',
        'auth/invalid-credential': 'Email or password is incorrect. Older GymBrain accounts can use Previous account sign-in below.',
        'auth/email-already-in-use': 'This email already has a Firebase account. Sign in or reset its password.',
        'auth/too-many-requests': 'Too many attempts. Please wait before trying again.',
        'auth/network-request-failed': 'Could not connect. Check your connection and try again.',
        'auth/weak-password': 'Choose a stronger password with at least 8 characters.',
        'auth/invalid-email': 'Enter a valid email address.',
    };
    return messages[code] || (code ? 'Sign-in could not be completed. Please try again.' : error instanceof Error ? error.message : 'Please try again.');
}
