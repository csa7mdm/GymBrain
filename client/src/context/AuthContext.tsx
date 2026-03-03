import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';

interface User {
    userId: string;
    email: string;
    token: string;
}

interface AuthContextType {
    user: User | null;
    setAuth: (userId: string, email: string, token: string) => void;
    logout: () => void;
    isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        const token = localStorage.getItem('gymbrain_token');
        const userId = localStorage.getItem('gymbrain_userId');
        const email = localStorage.getItem('gymbrain_email');
        if (token && userId && email) {
            setUser({ userId, email, token });
        }
    }, []);

    const setAuth = (userId: string, email: string, token: string) => {
        localStorage.setItem('gymbrain_token', token);
        localStorage.setItem('gymbrain_userId', userId);
        localStorage.setItem('gymbrain_email', email);
        setUser({ userId, email, token });
    };

    const logout = () => {
        localStorage.removeItem('gymbrain_token');
        localStorage.removeItem('gymbrain_userId');
        localStorage.removeItem('gymbrain_email');
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, setAuth, logout, isAuthenticated: !!user }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
