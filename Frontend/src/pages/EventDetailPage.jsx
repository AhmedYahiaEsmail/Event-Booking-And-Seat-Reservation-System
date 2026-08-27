// import { useEffect, useState } from 'react';
// import { useLocation, useNavigate, useParams } from 'react-router-dom';
// import { format, parseISO } from 'date-fns';
// import * as eventsApi from '../api/eventsApi';
// import * as reservationsApi from '../api/reservationsApi';
// import { ApiError } from '../api/client';
// import { useAuth } from '../context/AuthContext';
// import StatusBadge from '../components/ui/StatusBadge';
// import SeatGauge from '../components/ui/SeatGauge';
// import Input from '../components/ui/Input';
// import Button from '../components/ui/Button';
// import { ErrorAlert, SuccessAlert } from '../components/ui/Alert';
// import { useToast } from '../context/ToastContext';

// export default function EventDetailPage() {
//   const { id } = useParams();
//   const { isAuthenticated } = useAuth();
//   const { showToast } = useToast();
//   const navigate = useNavigate();
//   const location = useLocation();

//   const [event, setEvent] = useState(null);
//   const [isLoading, setIsLoading] = useState(true);
//   const [isNotFound, setIsNotFound] = useState(false);
//   const [fetchErrorMessage, setFetchErrorMessage] = useState(null);
//   const [refetchTrigger, setRefetchTrigger] = useState(0);

//   const [numberOfSeats, setNumberOfSeats] = useState(1);
//   const [isBooking, setIsBooking] = useState(false);
//   const [bookingErrorMessage, setBookingErrorMessage] = useState(null);
//   const [bookingErrorList, setBookingErrorList] = useState(null);

//   useEffect(() => {
//     let isCancelled = false;

//     async function fetchEvent() {
//       setIsLoading(true);
//       setIsNotFound(false);
//       setFetchErrorMessage(null);

//       try {
//         const data = await eventsApi.getEventById(id);
//         if (!isCancelled) {
//           setEvent(data);
//         }
//       } catch (error) {
//         if (isCancelled) return;

//         if (error instanceof ApiError && error.status === 404) {
//           setIsNotFound(true);
//         } else if (error instanceof ApiError) {
//           setFetchErrorMessage(error.message);
//         } else {
//           setFetchErrorMessage('Something went wrong loading this event.');
//         }
//       } finally {
//         if (!isCancelled) {
//           setIsLoading(false);
//         }
//       }
//     }

//     fetchEvent();

//     return () => {
//       isCancelled = true;
//     };
//   }, [id, refetchTrigger]);

//   function handleLoginRedirect() {
//     navigate('/login', { state: { from: location } });
//   }

//   async function handleReserve(formEvent) {
//     formEvent.preventDefault();
//     setIsBooking(true);
//     setBookingErrorMessage(null);
//     setBookingErrorList(null);

//     try {
//       await reservationsApi.reserveSeats({ eventId: id, numberOfSeats });
//       showToast(`Booked ${numberOfSeats} seat(s)!`);
//       setRefetchTrigger((current) => current + 1);
//     } catch (error) {
//       if (error instanceof ApiError) {
//         if (Array.isArray(error.errors) && error.errors.length > 0) {
//           setBookingErrorList(error.errors);
//         } else {
//           setBookingErrorMessage(error.message);
//         }
//       } else {
//         setBookingErrorMessage('Something went wrong. Please try again.');
//       }
//     } finally {
//       setIsBooking(false);
//     }
//   }

//   if (isLoading) {
//     return (
//       <div className="mx-auto max-w-2xl px-4 py-10">
//         <p className="text-sm text-ink-soft">Loading event...</p>
//       </div>
//     );
//   }

//   if (isNotFound) {
//     return (
//       <div className="mx-auto max-w-2xl px-4 py-10">
//         <p className="text-sm text-ink-soft">Event not found.</p>
//       </div>
//     );
//   }

//   if (fetchErrorMessage) {
//     return (
//       <div className="mx-auto max-w-2xl px-4 py-10">
//         <ErrorAlert message={fetchErrorMessage} />
//       </div>
//     );
//   }

//   const isPublished = event.status === 'Published';
//   const isSoldOut = event.availableSeats === 0;

//   return (
//     <div className="mx-auto max-w-2xl px-4 py-10">
//       <div className="overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
//         <div className="flex flex-col gap-3 p-7">
//           <div className="flex items-start justify-between gap-2">
//             <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">
//               {format(parseISO(event.startDateTime), 'EEEE, MMM d, yyyy · p')} –{' '}
//               {format(parseISO(event.endDateTime), 'p')}
//             </p>
//             <StatusBadge status={event.status} />
//           </div>

//           <h1 className="font-display text-2xl font-semibold text-ink">{event.title}</h1>

//           {event.description && (
//             <p className="leading-relaxed text-sm text-ink-soft">{event.description}</p>
//           )}

//           <div className="mt-2 grid grid-cols-1 gap-3 sm:grid-cols-2">
//             <div>
//               <p className="text-xs font-medium uppercase tracking-wide text-ink-faint">Location</p>
//               <p className="text-sm text-ink">{event.location}</p>
//             </div>
//             <div>
//               <p className="text-xs font-medium uppercase tracking-wide text-ink-faint">Speaker</p>
//               <p className="text-sm text-ink">{event.speakerName}</p>
//             </div>
//           </div>

//           {event.speakerBio && (
//             <p className="text-sm leading-relaxed text-ink-soft">{event.speakerBio}</p>
//           )}
//         </div>

//         {/* Ticket perforation divider */}
//         <div className="relative border-t border-dashed border-line">
//           <span className="absolute -left-3 -top-3 h-6 w-6 rounded-full bg-surface" />
//           <span className="absolute -right-3 -top-3 h-6 w-6 rounded-full bg-surface" />
//         </div>

//         <div className="flex flex-col gap-5 p-7">
//           <SeatGauge available={event.availableSeats} total={event.totalSeats} />

//           {!isPublished && (
//             <p className="text-sm text-ink-soft">
//               This event is not open for booking (status: {event.status}).
//             </p>
//           )}

//           {isPublished && !isAuthenticated && (
//             <Button onClick={handleLoginRedirect} className="w-fit">
//               Log in to book
//             </Button>
//           )}

//           {isPublished && isAuthenticated && isSoldOut && (
//             <p className="text-sm text-ink-soft">This event is sold out.</p>
//           )}

//           {isPublished && isAuthenticated && !isSoldOut && (
//             <form onSubmit={handleReserve} className="flex flex-col gap-4">
//               <div className="w-32">
//                 <Input
//                   label="Number of seats"
//                   id="numberOfSeats"
//                   type="number"
//                   min={1}
//                   max={event.availableSeats}
//                   value={numberOfSeats}
//                   onChange={(changeEvent) => setNumberOfSeats(Number(changeEvent.target.value))}
//                   required
//                 />
//               </div>

//               <ErrorAlert message={bookingErrorMessage} list={bookingErrorList} />

//               <Button type="submit" disabled={isBooking} className="w-fit">
//                 {isBooking ? 'Reserving...' : 'Reserve seats'}
//               </Button>
//             </form>
//           )}
//         </div>
//       </div>
//     </div>
//   );
// }

import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams, Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import * as eventsApi from '../api/eventsApi';
import * as reservationsApi from '../api/reservationsApi';
import { ApiError } from '../api/client';
import { useAuth } from '../context/AuthContext';
import StatusBadge from '../components/ui/StatusBadge';
import SeatGauge from '../components/ui/SeatGauge';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';
import { useToast } from '../context/ToastContext';
import { 
  Calendar, 
  Clock, 
  MapPin, 
  User, 
  ArrowLeft, 
  Ticket, 
  Sparkles,
  Info
} from 'lucide-react';

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
      showToast(`Booked ${numberOfSeats} seat(s) successfully!`);
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

  /* Skeleton Loading State */
  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 animate-pulse">
        <div className="h-6 w-32 bg-surface rounded mb-6" />
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">
            <div className="h-8 w-3/4 bg-surface rounded-lg" />
            <div className="h-4 w-1/2 bg-surface rounded" />
            <div className="h-32 bg-surface rounded-xl" />
          </div>
          <div className="h-64 bg-surface rounded-2xl" />
        </div>
      </div>
    );
  }

  /* 404 Not Found State */
  if (isNotFound) {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-surface text-ink-soft">
          <Info className="h-6 w-6" />
        </div>
        <h2 className="text-xl font-semibold text-ink">Event Not Found</h2>
        <p className="mt-1 text-sm text-ink-soft">The event you are looking for does not exist or has been removed.</p>
        <Link to="/" className="mt-6 inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline">
          <ArrowLeft className="h-4 w-4" /> Back to events
        </Link>
      </div>
    );
  }

  /* Fetch Error State */
  if (fetchErrorMessage) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-10">
        <ErrorAlert message={fetchErrorMessage} />
        <Link to="/" className="mt-4 inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline">
          <ArrowLeft className="h-4 w-4" /> Back to events
        </Link>
      </div>
    );
  }

  const isPublished = event.status === 'Published';
  const isSoldOut = event.availableSeats === 0;

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      {/* Back Button */}
      <Link
        to="/"
        className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-ink-soft transition hover:text-ink"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to events
      </Link>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Main Details (2 Columns on Desktop) */}
        <div className="lg:col-span-2 space-y-8">
          <div>
            <div className="flex items-center gap-3 mb-3">
              <StatusBadge status={event.status} />
              <span className="text-xs font-medium text-ink-faint uppercase tracking-wider">
                Event Details
              </span>
            </div>
            <h1 className="font-display text-3xl font-extrabold text-ink sm:text-4xl tracking-tight">
              {event.title}
            </h1>
          </div>

          {/* Quick Info Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 rounded-2xl border border-line bg-surface/50 p-4">
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                <Calendar className="h-5 w-5" />
              </div>
              <div>
                <p className="text-xs font-medium text-ink-faint uppercase">Date & Time</p>
                <p className="text-sm font-semibold text-ink mt-0.5">
                  {format(parseISO(event.startDateTime), 'EEEE, MMM d, yyyy')}
                </p>
                <p className="text-xs text-ink-soft">
                  {format(parseISO(event.startDateTime), 'p')} – {format(parseISO(event.endDateTime), 'p')}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                <MapPin className="h-5 w-5" />
              </div>
              <div>
                <p className="text-xs font-medium text-ink-faint uppercase">Location</p>
                <p className="text-sm font-semibold text-ink mt-0.5">{event.location}</p>
                <p className="text-xs text-ink-soft">In-person session</p>
              </div>
            </div>
          </div>

          {/* About Section */}
          {event.description && (
            <div className="space-y-3">
              <h2 className="text-lg font-bold text-ink">About this Event</h2>
              <p className="text-sm leading-relaxed text-ink-soft whitespace-pre-line">
                {event.description}
              </p>
            </div>
          )}

          {/* Speaker Card */}
          {event.speakerName && (
            <div className="rounded-2xl border border-line bg-paper p-5 space-y-3">
              <div className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-surface text-ink-soft border border-line">
                  <User className="h-6 w-6" />
                </div>
                <div>
                  <p className="text-xs font-medium text-ink-faint uppercase">Featured Speaker</p>
                  <h3 className="text-base font-bold text-ink">{event.speakerName}</h3>
                </div>
              </div>
              {event.speakerBio && (
                <p className="text-sm text-ink-soft leading-relaxed pt-2 border-t border-line/60">
                  {event.speakerBio}
                </p>
              )}
            </div>
          )}
        </div>

        {/* Sidebar Sticky Booking Card */}
        <div className="lg:col-span-1">
          <div className="sticky top-24 rounded-2xl border border-line bg-paper p-6 shadow-xl shadow-ink/5 space-y-6">
            <div className="flex items-center gap-2 text-ink font-semibold border-b border-line pb-4">
              <Ticket className="h-5 w-5 text-primary" />
              <span>Reserve Your Ticket</span>
            </div>

            <SeatGauge available={event.availableSeats} total={event.totalSeats} />

            {!isPublished && (
              <div className="rounded-xl bg-surface p-4 text-center text-sm text-ink-soft">
                This event is not currently open for booking (Status: {event.status}).
              </div>
            )}

            {isPublished && !isAuthenticated && (
              <div className="space-y-3">
                <p className="text-xs text-ink-soft text-center">
                  You need to be logged in to book seats for this event.
                </p>
                <Button onClick={handleLoginRedirect} variant="primary" className="w-full">
                  Log in to book
                </Button>
              </div>
            )}

            {isPublished && isAuthenticated && isSoldOut && (
              <div className="rounded-xl bg-surface p-4 text-center text-sm font-medium text-ink-soft">
                Sorry, this event is completely sold out!
              </div>
            )}

            {isPublished && isAuthenticated && !isSoldOut && (
              <form onSubmit={handleReserve} className="space-y-4">
                <Input
                  label="Number of Seats"
                  id="numberOfSeats"
                  type="number"
                  min={1}
                  max={event.availableSeats}
                  value={numberOfSeats}
                  onChange={(e) => setNumberOfSeats(Number(e.target.value))}
                  required
                />

                <ErrorAlert message={bookingErrorMessage} list={bookingErrorList} />

                <Button type="submit" disabled={isBooking} variant="primary" className="w-full py-3">
                  {isBooking ? 'Reserving Seats...' : 'Confirm Reservation'}
                </Button>
              </form>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}