import { createContext, useContext, useEffect, useState } from 'react';
import * as authApi from '../api/authApi';
import { setAuthToken, setUnauthorizedHandler } from '../api/client';

const AUTH_STORAGE_KEY = 'auth';

const AuthContext = createContext(null);

function readStoredAuth() {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    // Corrupted/unparseable blob — treat as logged out rather than throwing.
    return null;
  }
}

function splitStoredAuth(stored) {
  if (!stored) return { user: null, token: null };
  const { token, ...user } = stored;
  return { user, token: token ?? null };
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);

  // Hydrate from localStorage on mount, and wire up the client's 401 handler
  // so an expired/invalid token clears local auth state automatically.
  useEffect(() => {
    const stored = readStoredAuth();
    const { user: hydratedUser, token: hydratedToken } = splitStoredAuth(stored);

    if (hydratedToken) {
      setUser(hydratedUser);
      setToken(hydratedToken);
      setAuthToken(hydratedToken);
    }

    setUnauthorizedHandler(() => {
      clearAuth();
    });
  }, []);

  function persistAuth(result) {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(result));
    const { user: nextUser, token: nextToken } = splitStoredAuth(result);
    setUser(nextUser);
    setToken(nextToken);
    setAuthToken(nextToken);
  }

  function clearAuth() {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    setUser(null);
    setToken(null);
    setAuthToken(null);
  }

  async function login(email, password) {
    const result = await authApi.login({ email, password });
    persistAuth(result);
    return result;
  }

  async function register(firstName, lastName, email, password) {
    const result = await authApi.register({ firstName, lastName, email, password });
    persistAuth(result);
    return result;
  }

  function logout() {
    clearAuth();
  }

  const isAuthenticated = !!token;
  const isAdmin = user?.role === 'Admin';

  const value = {
    user,
    token,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }
  return context;
}
