import { client } from './client';

// Everything under /reservations requires auth; client.js attaches the
// bearer token automatically whenever one has been set via setAuthToken().

function toQueryString(params = {}) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '');
  if (entries.length === 0) return '';
  return `?${new URLSearchParams(entries).toString()}`;
}

export function reserveSeats({ eventId, numberOfSeats }) {
  return client.post('/reservations', { eventId, numberOfSeats });
}

export function cancelReservation(id) {
  return client.post(`/reservations/${id}/cancel`);
}

export function getMyReservations(queryParams = {}) {
  return client.get(`/reservations/me${toQueryString(queryParams)}`);
}
