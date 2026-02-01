import { createContext, useContext, useState, ReactNode, useCallback } from 'react';
import type { User, LoginRequest, RegisterRequest } from '@/types/auth';
import { authApi } from '@/api/auth';
import { ApiError } from '@/api/client';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem('currentUser');
    return stored ? JSON.parse(stored) : null;
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const login = useCallback(async (credentials: LoginRequest) => {
    setLoading(true);
    setError('');
    try {
      const response = await authApi.login(credentials);
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('currentUser', JSON.stringify({
        username: response.username,
        email: response.email,
        role: response.role === 'Admin' ? 'Admin' : 'User',
      }));
      setUser({
        username: response.username,
        email: response.email,
        role: response.role === 'Admin' ? 'Admin' : 'User',
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    setLoading(true);
    setError('');
    try {
      await authApi.register(data);
    } catch (err) {
      // Don't set error here for validation errors - let the component handle it
      // Only set error for non-validation errors
      if (!(err instanceof ApiError)) {
        setError(err instanceof Error ? err.message : 'Registration failed');
      }
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    setUser(null);
    setError('');
  }, []);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, error, login, register, logout, clearError }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
