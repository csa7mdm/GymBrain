import { createContext, useContext } from 'react';

export interface User {
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

export const AuthContext = createContext<AuthContextType | null>(null);

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
