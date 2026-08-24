import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import * as eventsApi from '../api/eventsApi';
import * as reservationsApi from '../api/reservationsApi';
import { ApiError } from '../api/client';
import { useAuth } from '../context/AuthContext';
import StatusBadge from '../components/ui/StatusBadge';
import SeatGauge from '../components/ui/SeatGauge';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { ErrorAlert, SuccessAlert } from '../components/ui/Alert';
import { useToast } from '../context/ToastContext';

export default function EventDetailPage() {
  const { id } = useParams();
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();
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

    try {
      await reservationsApi.reserveSeats({ eventId: id, numberOfSeats });
      showToast(`Booked ${numberOfSeats} seat(s)!`);
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
      <div className="mx-auto max-w-2xl px-4 py-10">
        <p className="text-sm text-ink-soft">Loading event...</p>
      </div>
    );
  }

  if (isNotFound) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-10">
        <p className="text-sm text-ink-soft">Event not found.</p>
      </div>
    );
  }

  if (fetchErrorMessage) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-10">
        <ErrorAlert message={fetchErrorMessage} />
      </div>
    );
  }

  const isPublished = event.status === 'Published';
  const isSoldOut = event.availableSeats === 0;

  return (
    <div className="mx-auto max-w-2xl px-4 py-10">
      <div className="overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
        <div className="flex flex-col gap-3 p-7">
          <div className="flex items-start justify-between gap-2">
            <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">
              {format(parseISO(event.startDateTime), 'EEEE, MMM d, yyyy · p')} –{' '}
              {format(parseISO(event.endDateTime), 'p')}
            </p>
            <StatusBadge status={event.status} />
          </div>

          <h1 className="font-display text-2xl font-semibold text-ink">{event.title}</h1>

          {event.description && (
            <p className="leading-relaxed text-sm text-ink-soft">{event.description}</p>
          )}

          <div className="mt-2 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-ink-faint">Location</p>
              <p className="text-sm text-ink">{event.location}</p>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-ink-faint">Speaker</p>
              <p className="text-sm text-ink">{event.speakerName}</p>
            </div>
          </div>

          {event.speakerBio && (
            <p className="text-sm leading-relaxed text-ink-soft">{event.speakerBio}</p>
          )}
        </div>

        {/* Ticket perforation divider */}
        <div className="relative border-t border-dashed border-line">
          <span className="absolute -left-3 -top-3 h-6 w-6 rounded-full bg-surface" />
          <span className="absolute -right-3 -top-3 h-6 w-6 rounded-full bg-surface" />
        </div>

        <div className="flex flex-col gap-5 p-7">
          <SeatGauge available={event.availableSeats} total={event.totalSeats} />

          {!isPublished && (
            <p className="text-sm text-ink-soft">
              This event is not open for booking (status: {event.status}).
            </p>
          )}

          {isPublished && !isAuthenticated && (
            <Button onClick={handleLoginRedirect} className="w-fit">
              Log in to book
            </Button>
          )}

          {isPublished && isAuthenticated && isSoldOut && (
            <p className="text-sm text-ink-soft">This event is sold out.</p>
          )}

          {isPublished && isAuthenticated && !isSoldOut && (
            <form onSubmit={handleReserve} className="flex flex-col gap-4">
              <div className="w-32">
                <Input
                  label="Number of seats"
                  id="numberOfSeats"
                  type="number"
                  min={1}
                  max={event.availableSeats}
                  value={numberOfSeats}
                  onChange={(changeEvent) => setNumberOfSeats(Number(changeEvent.target.value))}
                  required
                />
              </div>

              <ErrorAlert message={bookingErrorMessage} list={bookingErrorList} />

              <Button type="submit" disabled={isBooking} className="w-fit">
                {isBooking ? 'Reserving...' : 'Reserve seats'}
              </Button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}