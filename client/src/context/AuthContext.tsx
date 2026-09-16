import { useState, type ReactNode } from 'react';
import { AuthContext, type User } from './auth';

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(() => {
        // Remove credentials retained by older onboarding builds.
        try {
            const profile = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
            if ('apiKey' in profile) {
                delete profile.apiKey;
                localStorage.setItem('gymbrain_profile', JSON.stringify(profile));
            }
        } catch { localStorage.removeItem('gymbrain_profile'); }
        const token = localStorage.getItem('gymbrain_token');
        const userId = localStorage.getItem('gymbrain_userId');
        const email = localStorage.getItem('gymbrain_email');
        return token && userId && email ? { userId, email, token } : null;
    });

    const setAuth = (userId: string, email: string, token: string) => {
        localStorage.setItem('gymbrain_token', token);
        localStorage.setItem('gymbrain_userId', userId);
        localStorage.setItem('gymbrain_email', email);
        setUser({ userId, email, token });
    };

    const logout = () => {
        Object.keys(localStorage).forEach(key => {
            if (key.startsWith('gymbrain_')) {
                localStorage.removeItem(key);
            }
        });
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, setAuth, logout, isAuthenticated: !!user }}>
            {children}
        </AuthContext.Provider>
    );
}
