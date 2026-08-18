import { client } from './client';

// Both endpoints return { userId, firstName, lastName, email, role, token }
// unwrapped from the envelope by client.js.

export function register({ firstName, lastName, email, password }) {
  return client.post('/auth/register', { firstName, lastName, email, password });
}

export function login({ email, password }) {
  return client.post('/auth/login', { email, password });
}
