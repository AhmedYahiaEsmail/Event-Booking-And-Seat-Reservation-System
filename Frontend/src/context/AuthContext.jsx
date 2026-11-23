import { createContext, useContext, useEffect, useState } from 'react';
import * as authApi from '../api/authApi';
import { setAuthToken, setRefreshToken, setUnauthorizedHandler, setTokensRefreshedHandler } from '../api/client';

const AUTH_STORAGE_KEY = 'auth';

const AuthContext = createContext(null);

function readStoredAuth() {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    // Corrupted/unparseable blob â€” treat as logged out rather than throwing.
    return null;
  }
}

function splitStoredAuth(stored) {
  if (!stored) return { user: null, token: null, refreshToken: null };
  const { token, refreshToken, ...user } = stored;
  return { user, token: token ?? null, refreshToken: refreshToken ?? null };
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);

  // Hydrate from localStorage on mount, and wire up client.js's callbacks so:
  //  - an unrecoverable 401 clears local auth state automatically
  //  - a silent refresh triggered from client.js gets persisted here too
  useEffect(() => {
    const stored = readStoredAuth();
    const { user: hydratedUser, token: hydratedToken, refreshToken: hydratedRefreshToken } = splitStoredAuth(stored);

    if (hydratedToken) {
      setUser(hydratedUser);
      setToken(hydratedToken);
      setAuthToken(hydratedToken);
      setRefreshToken(hydratedRefreshToken);
    }

    setUnauthorizedHandler(() => {
      clearAuth();
    });

    // client.js's doRefresh() returns a full AuthResponse (same shape as
    // login/register), so it can be persisted exactly the same way.
    setTokensRefreshedHandler((result) => {
      persistAuth(result);
    });
  }, []);

  function persistAuth(result) {
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(result));
    const { user: nextUser, token: nextToken, refreshToken: nextRefreshToken } = splitStoredAuth(result);
    setUser(nextUser);
    setToken(nextToken);
    setAuthToken(nextToken);
    setRefreshToken(nextRefreshToken);
  }

  function clearAuth() {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    setUser(null);
    setToken(null);
    setAuthToken(null);
    setRefreshToken(null);
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
    // Best-effort server-side revocation, fired before clearAuth() below so the
    // request still carries a valid Authorization header. Local logout does not
    // wait on or depend on this succeeding (e.g. the user is offline, or the
    // refresh token already expired) â€” auth state is cleared regardless.
    const stored = readStoredAuth();
    const currentRefreshToken = stored?.refreshToken;

    if (currentRefreshToken) {
      authApi.revokeToken(currentRefreshToken).catch(() => {});
    }

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