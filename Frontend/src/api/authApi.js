import { client } from './client';

// register/login/refreshToken all return the full AuthResponse envelope:
// { userId, firstName, lastName, email, role, token, refreshToken, refreshTokenExpiresAt }
// unwrapped from the envelope by client.js.

export function register({ firstName, lastName, email, password }) {
  return client.post('/auth/register', { firstName, lastName, email, password });
}

export function login({ email, password }) {
  return client.post('/auth/login', { email, password });
}

// Not used by the automatic silent-refresh flow (that lives entirely inside
// client.js so it can intercept any 401 transparently) â€” kept here for any
// call site that wants to trigger a refresh explicitly.
export function refreshToken(refreshTokenValue) {
  return client.post('/auth/refresh-token', { refreshToken: refreshTokenValue });
}

export function revokeToken(refreshTokenValue) {
  return client.post('/auth/revoke-token', { refreshToken: refreshTokenValue });
}