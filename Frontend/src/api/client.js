// Thin fetch wrapper around the Event Booking API.
//
// Two module-level pieces of mutable state live here (not in React) so that
// this file has no dependency on AuthContext and there's no import cycle:
//   - authToken:      the current JWT, set by AuthContext via setAuthToken()
//   - onUnauthorized: a callback AuthContext registers via
//                      setUnauthorizedHandler() so this client can clear auth
//                      state on a 401 without importing the context itself.

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

let authToken = null;
let onUnauthorized = null;

export function setAuthToken(token) {
  authToken = token;
}

export function setUnauthorizedHandler(handler) {
  onUnauthorized = handler;
}

// Custom error carrying the API envelope's failure details, so callers can
// distinguish "request failed for a reason the backend explained" from a
// network-level failure.
export class ApiError extends Error {
  constructor(message, { errors = null, status = null } = {}) {
    super(message);
    this.name = 'ApiError';
    this.errors = errors;
    this.status = status;
  }
}

async function request(path, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (authToken) {
    headers.Authorization = `Bearer ${authToken}`;
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers,
  });

  // The backend always responds with the { isSuccess, message, data, errors }
  // envelope, including on error responses, so we can always attempt to
  // parse it. A 204 No Content has no body to parse.
  let envelope = null;
  if (response.status !== 204) {
    envelope = await response.json().catch(() => null);
  }

  if (response.status === 401) {
    // Let AuthContext's registered handler clear stale auth state (localStorage
    // + in-memory user/token) before we throw, so an expired/invalid token
    // never lingers around after a 401. No client-side JWT expiry parsing is
    // done anywhere — the backend's 401 is the sole source of truth.
    onUnauthorized?.();
  }

  if (!response.ok || envelope?.isSuccess === false) {
    const message = envelope?.message ?? `Request failed with status ${response.status}.`;
    throw new ApiError(message, { errors: envelope?.errors ?? null, status: response.status });
  }

  return envelope?.data ?? null;
}

export const client = {
  get: (path, options) => request(path, { ...options, method: 'GET' }),
  post: (path, body, options) =>
    request(path, { ...options, method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: (path, body, options) =>
    request(path, { ...options, method: 'PUT', body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: (path, options) => request(path, { ...options, method: 'DELETE' }),
};
