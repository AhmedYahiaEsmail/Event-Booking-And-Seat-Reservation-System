import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import * as reservationsApi from '../api/reservationsApi';
import { ApiError } from '../api/client';

export default function DashboardPage() {
  const [reservations, setReservations] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);
  const [cancelingId, setCancelingId] = useState(null);

  useEffect(() => {
    let isCancelled = false;

    async function fetchReservations() {
      setIsLoading(true);
      setErrorMessage(null);
      setErrorList(null);

      try {
        const data = await reservationsApi.getMyReservations({ pageSize: 100 });
        if (!isCancelled) {
          setReservations(data ?? []);
        }
      } catch (error) {
        if (isCancelled) return;

        if (error instanceof ApiError) {
          if (Array.isArray(error.errors) && error.errors.length > 0) {
            setErrorList(error.errors);
          } else {
            setErrorMessage(error.message);
          }
        } else {
          setErrorMessage('Something went wrong. Please try again.');
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    fetchReservations();

    return () => {
      isCancelled = true;
    };
  }, [refetchTrigger]);

  // Derived on every render rather than stored as state — the backend's
  // fromDate/toDate filters apply to BookingDateTime, not the event's date, so
  // upcoming/past has to be computed here against eventStartDateTime.
  const now = new Date();
  const upcoming = reservations.filter(
    (r) => r.status === 'Confirmed' && new Date(r.eventStartDateTime) >= now
  );
  const past = reservations.filter(
    (r) => !(r.status === 'Confirmed' && new Date(r.eventStartDateTime) >= now)
  );

  async function handleCancel(reservationId) {
    const confirmed = window.confirm('Cancel this reservation?');
    if (!confirmed) return;

    setCancelingId(reservationId);
    setErrorMessage(null);
    setErrorList(null);

    try {
      await reservationsApi.cancelReservation(reservationId);
      setRefetchTrigger((current) => current + 1);
    } catch (error) {
      if (error instanceof ApiError) {
        if (Array.isArray(error.errors) && error.errors.length > 0) {
          setErrorList(error.errors);
        } else {
          setErrorMessage(error.message);
        }
      } else {
        setErrorMessage('Something went wrong. Please try again.');
      }
    } finally {
      setCancelingId(null);
    }
  }

  if (isLoading) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8">
        <p className="text-sm text-gray-600">Loading reservations...</p>
      </div>
    );
  }

  if (errorList) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8">
        <ul className="list-inside list-disc text-sm text-red-600">
          {errorList.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
      </div>
    );
  }

  if (errorMessage) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8">
        <p className="text-sm text-red-600">{errorMessage}</p>
      </div>
    );
  }

  if (upcoming.length === 0 && past.length === 0) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-8">
        <h1 className="mb-4 text-xl font-semibold text-gray-900">My Reservations</h1>
        <p className="text-sm text-gray-600">
          You haven&apos;t booked anything yet.{' '}
          <Link to="/" className="font-medium text-gray-900 hover:underline">
            Browse events
          </Link>
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-6 text-xl font-semibold text-gray-900">My Reservations</h1>

      <section className="mb-8">
        <h2 className="mb-3 text-lg font-semibold text-gray-900">Upcoming Reservations</h2>
        {upcoming.length === 0 ? (
          <p className="text-sm text-gray-600">No upcoming reservations.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {upcoming.map((reservation) => (
              <ReservationRow
                key={reservation.id}
                reservation={reservation}
                showCancelButton
                isCanceling={cancelingId === reservation.id}
                onCancel={() => handleCancel(reservation.id)}
              />
            ))}
          </ul>
        )}
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold text-gray-900">Past Reservations</h2>
        {past.length === 0 ? (
          <p className="text-sm text-gray-600">No past reservations.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {past.map((reservation) => (
              <ReservationRow key={reservation.id} reservation={reservation} />
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

function ReservationRow({ reservation, showCancelButton = false, isCanceling = false, onCancel }) {
  const {
    eventId,
    eventTitle,
    eventStartDateTime,
    eventLocation,
    numberOfSeats,
    status,
  } = reservation;

  return (
    <li className="flex flex-col gap-1 rounded-md border border-gray-200 p-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex flex-col gap-1">
        <Link to={`/events/${eventId}`} className="text-sm font-medium text-gray-900 hover:underline">
          {eventTitle || 'Deleted event'}
        </Link>
        <p className="text-sm text-gray-600">{format(parseISO(eventStartDateTime), 'PPp')}</p>
        <p className="text-sm text-gray-600">{eventLocation}</p>
        <p className="text-sm text-gray-600">
          {numberOfSeats} seat(s) — {status}
        </p>
      </div>

      {showCancelButton && (
        <button
          type="button"
          onClick={onCancel}
          disabled={isCanceling}
          className="w-fit rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isCanceling ? 'Cancelling...' : 'Cancel'}
        </button>
      )}
    </li>
  );
}
