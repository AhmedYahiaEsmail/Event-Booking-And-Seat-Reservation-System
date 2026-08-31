// Thin fetch wrapper around the Event Booking API.
//
// Module-level mutable state lives here (not in React) so this file has no
// dependency on AuthContext and there's no import cycle:
//   - authToken / refreshToken: current tokens, set by AuthContext via
//                                setAuthToken() / setRefreshToken()
//   - onUnauthorized:           callback AuthContext registers via
//                                setUnauthorizedHandler() so this client can
//                                clear auth state when a 401 truly means
//                                "you're logged out" (not just "access token
//                                expired, refresh worked")
//   - onTokensRefreshed:        callback AuthContext registers via
//                                setTokensRefreshedHandler() so a silent
//                                refresh triggered from here can be persisted
//                                back into localStorage + React state

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

let authToken = null;
let refreshTokenValue = null;
let onUnauthorized = null;
let onTokensRefreshed = null;

// A 401 from any of these IS the answer (bad credentials, or a bad/expired/revoked
// refresh token) â€” never a signal to attempt a silent refresh-and-retry.
const NO_REFRESH_PATHS = ['/auth/login', '/auth/register', '/auth/refresh-token'];

export function setAuthToken(token) {
  authToken = token;
}

export function setRefreshToken(token) {
  refreshTokenValue = token;
}

export function setUnauthorizedHandler(handler) {
  onUnauthorized = handler;
}

export function setTokensRefreshedHandler(handler) {
  onTokensRefreshed = handler;
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

// Ensures only one /auth/refresh-token call is ever in flight at a time. If several
// requests all hit a 401 at once (e.g. right after the access token expires), they
// share this single promise instead of each firing their own refresh â€” which would
// race against the backend's rotation: only the first would succeed, and it would
// invalidate the refresh token before the others got to use it.
let refreshPromise = null;

function refreshAccessToken() {
  if (!refreshPromise) {
    refreshPromise = doRefresh().finally(() => {
      refreshPromise = null;
    });
  }
  return refreshPromise;
}

async function doRefresh() {
  const response = await fetch(`${BASE_URL}/auth/refresh-token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: refreshTokenValue }),
  });

  const envelope = await response.json().catch(() => null);

  if (!response.ok || envelope?.isSuccess === false) {
    throw new ApiError(envelope?.message ?? 'Session expired.', { status: response.status });
  }

  const result = envelope.data;
  authToken = result.token;
  refreshTokenValue = result.refreshToken;
  onTokensRefreshed?.(result);

  return result.token;
}

async function request(path, options = {}, isRetry = false) {
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
    const canAttemptRefresh = !isRetry && !!refreshTokenValue && !NO_REFRESH_PATHS.includes(path);

    if (canAttemptRefresh) {
      try {
        await refreshAccessToken();
        // Refresh succeeded: replay the original request exactly once with the
        // new access token, short-circuiting the failure handling below.
        return request(path, options, true);
      } catch {
        // Refresh itself failed (refresh token expired/revoked/reused) â€” fall
        // through to normal 401 handling, which clears auth state.
      }
    }

    // Either this was already a retry, there was no refresh token to try, this
    // *is* one of the auth endpoints, or the refresh attempt above failed: this
    // 401 is final. Let AuthContext's registered handler clear stale auth state.
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