import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import * as eventsApi from '../api/eventsApi';
import * as reservationsApi from '../api/reservationsApi';
import { ApiError } from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function EventDetailPage() {
  const { id } = useParams();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [event, setEvent] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isNotFound, setIsNotFound] = useState(false);
  const [fetchErrorMessage, setFetchErrorMessage] = useState(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);

  const [numberOfSeats, setNumberOfSeats] = useState(1);
  const [isBooking, setIsBooking] = useState(false);
  const [bookingErrorMessage, setBookingErrorMessage] = useState(null);
  const [bookingErrorList, setBookingErrorList] = useState(null);
  const [bookingSuccessMessage, setBookingSuccessMessage] = useState(null);

  useEffect(() => {
    let isCancelled = false;

    async function fetchEvent() {
      setIsLoading(true);
      setIsNotFound(false);
      setFetchErrorMessage(null);

      try {
        const data = await eventsApi.getEventById(id);
        if (!isCancelled) {
          setEvent(data);
        }
      } catch (error) {
        if (isCancelled) return;

        if (error instanceof ApiError && error.status === 404) {
          setIsNotFound(true);
        } else if (error instanceof ApiError) {
          setFetchErrorMessage(error.message);
        } else {
          setFetchErrorMessage('Something went wrong loading this event.');
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    fetchEvent();

    return () => {
      isCancelled = true;
    };
  }, [id, refetchTrigger]);

  function handleLoginRedirect() {
    navigate('/login', { state: { from: location } });
  }

  async function handleReserve(formEvent) {
    formEvent.preventDefault();
    setIsBooking(true);
    setBookingErrorMessage(null);
    setBookingErrorList(null);
    setBookingSuccessMessage(null);

    try {
      await reservationsApi.reserveSeats({ eventId: id, numberOfSeats });
      setBookingSuccessMessage(`Booked ${numberOfSeats} seat(s)!`);
      setRefetchTrigger((current) => current + 1);
    } catch (error) {
      if (error instanceof ApiError) {
        if (Array.isArray(error.errors) && error.errors.length > 0) {
          setBookingErrorList(error.errors);
        } else {
          setBookingErrorMessage(error.message);
        }
      } else {
        setBookingErrorMessage('Something went wrong. Please try again.');
      }
    } finally {
      setIsBooking(false);
    }
  }

  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <p className="text-sm text-gray-600">Loading event...</p>
      </div>
    );
  }

  if (isNotFound) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <p className="text-sm text-gray-600">Event not found.</p>
      </div>
    );
  }

  if (fetchErrorMessage) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <p className="text-sm text-red-600">{fetchErrorMessage}</p>
      </div>
    );
  }

  const dateRange = `${format(parseISO(event.startDateTime), 'PPp')} – ${format(
    parseISO(event.endDateTime),
    'PPp'
  )}`;
  const isPublished = event.status === 'Published';
  const isSoldOut = event.availableSeats === 0;

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <div className="flex flex-col gap-2">
        <h1 className="text-xl font-semibold text-gray-900">{event.title}</h1>
        {event.description && (
          <p className="text-sm text-gray-700 leading-relaxed my-1">
            {event.description}
           </p>
        )}
        <p className="text-sm text-gray-600">{dateRange}</p>
        <p className="text-sm text-gray-600">{event.location}</p>
        <p className="text-sm text-gray-600">{event.speakerName}</p>
        {event.speakerBio && <p className="text-sm text-gray-600">{event.speakerBio}</p>}
        <p className="text-sm font-medium text-gray-700">
          {event.availableSeats} / {event.totalSeats} seats available
        </p>
        <p className="text-sm text-gray-600">Status: {event.status}</p>
      </div>

      <div className="mt-6 rounded-md border border-gray-200 p-4">
        {!isPublished && (
          <p className="text-sm text-gray-600">
            This event is not open for booking (status: {event.status}).
          </p>
        )}

        {isPublished && !isAuthenticated && (
          <button
            type="button"
            onClick={handleLoginRedirect}
            className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700"
          >
            Log in to book
          </button>
        )}

        {isPublished && isAuthenticated && isSoldOut && (
          <p className="text-sm text-gray-600">Sold out</p>
        )}

        {isPublished && isAuthenticated && !isSoldOut && (
          <form onSubmit={handleReserve} className="flex flex-col gap-3">
            <div className="flex flex-col gap-1">
              <label htmlFor="numberOfSeats" className="text-sm font-medium text-gray-700">
                Number of seats
              </label>
              <input
                id="numberOfSeats"
                type="number"
                min={1}
                max={event.availableSeats}
                value={numberOfSeats}
                onChange={(changeEvent) => setNumberOfSeats(Number(changeEvent.target.value))}
                required
                className="w-24 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
              />
            </div>

            {bookingErrorList && (
              <ul className="list-inside list-disc text-sm text-red-600">
                {bookingErrorList.map((message) => (
                  <li key={message}>{message}</li>
                ))}
              </ul>
            )}

            {bookingErrorMessage && <p className="text-sm text-red-600">{bookingErrorMessage}</p>}
            {bookingSuccessMessage && <p className="text-sm text-green-600">{bookingSuccessMessage}</p>}

            <button
              type="submit"
              disabled={isBooking}
              className="w-fit rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isBooking ? 'Reserving...' : 'Reserve'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
