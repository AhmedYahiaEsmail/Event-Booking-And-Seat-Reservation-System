import { client } from './client';

// GET /events, GET /events/upcoming, and GET /events/{id} are public — no
// token is required, though client.js will still attach one if the user
// happens to be logged in (harmless either way).

function toQueryString(params = {}) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '');
  if (entries.length === 0) return '';
  return `?${new URLSearchParams(entries).toString()}`;
}

export function getEvents(queryParams = {}) {
  return client.get(`/events${toQueryString(queryParams)}`);
}

export function getUpcomingEvents(queryParams = {}) {
  return client.get(`/events/upcoming${toQueryString(queryParams)}`);
}

export function getEventById(id) {
  return client.get(`/events/${id}`);
}

// Admin-only (server-enforced via the JWT's role claim).

export function createEvent(payload) {
  return client.post('/events', payload);
}

export function updateEvent(id, payload) {
  return client.put(`/events/${id}`, payload);
}

export function deleteEvent(id) {
  return client.delete(`/events/${id}`);
}

export function publishEvent(id) {
  return client.post(`/events/${id}/publish`);
}

export function cancelEvent(id) {
  return client.post(`/events/${id}/cancel`);
}

export function completeEvent(id) {
  return client.post(`/events/${id}/complete`);
}
