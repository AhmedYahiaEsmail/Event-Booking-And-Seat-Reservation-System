// import { useEffect, useState } from 'react';
// import { Link } from 'react-router-dom';
// import { format, parseISO } from 'date-fns';
// import * as reservationsApi from '../api/reservationsApi';
// import { ApiError } from '../api/client';
// import StatusBadge from '../components/ui/StatusBadge';
// import Button from '../components/ui/Button';
// import { ErrorAlert } from '../components/ui/Alert';
// import EmptyState from '../components/ui/EmptyState';
// import { useToast } from '../context/ToastContext';

// export default function DashboardPage() {
//   const [reservations, setReservations] = useState([]);
//   const [isLoading, setIsLoading] = useState(true);
//   const [errorMessage, setErrorMessage] = useState(null);
//   const [errorList, setErrorList] = useState(null);
//   const [refetchTrigger, setRefetchTrigger] = useState(0);
//   const [cancelingId, setCancelingId] = useState(null);
//   const { showToast } = useToast();

//   useEffect(() => {
//     let isCancelled = false;

//     async function fetchReservations() {
//       setIsLoading(true);
//       setErrorMessage(null);
//       setErrorList(null);

//       try {
//         const data = await reservationsApi.getMyReservations({ pageSize: 100 });
//         if (!isCancelled) {
//           setReservations(data ?? []);
//         }
//       } catch (error) {
//         if (isCancelled) return;

//         if (error instanceof ApiError) {
//           if (Array.isArray(error.errors) && error.errors.length > 0) {
//             setErrorList(error.errors);
//           } else {
//             setErrorMessage(error.message);
//           }
//         } else {
//           setErrorMessage('Something went wrong. Please try again.');
//         }
//       } finally {
//         if (!isCancelled) {
//           setIsLoading(false);
//         }
//       }
//     }

//     fetchReservations();

//     return () => {
//       isCancelled = true;
//     };
//   }, [refetchTrigger]);

//   const now = new Date();
//   const upcoming = reservations.filter(
//     (r) => r.status === 'Confirmed' && new Date(r.eventStartDateTime) >= now
//   );
//   const past = reservations.filter(
//     (r) => !(r.status === 'Confirmed' && new Date(r.eventStartDateTime) >= now)
//   );

//   async function handleCancel(reservationId) {
//     const confirmed = window.confirm('Cancel this reservation?');
//     if (!confirmed) return;

//     setCancelingId(reservationId);
//     setErrorMessage(null);
//     setErrorList(null);

//     try {
//       await reservationsApi.cancelReservation(reservationId);
//       showToast('Reservation cancelled.');
//       setRefetchTrigger((current) => current + 1);
//     } catch (error) {
//       if (error instanceof ApiError) {
//         if (Array.isArray(error.errors) && error.errors.length > 0) {
//           setErrorList(error.errors);
//         } else {
//           setErrorMessage(error.message);
//         }
//       } else {
//         setErrorMessage('Something went wrong. Please try again.');
//       }
//     } finally {
//       setCancelingId(null);
//     }
//   }

//   if (isLoading) {
//     return (
//       <div className="mx-auto max-w-3xl px-4 py-10">
//         <p className="text-sm text-ink-soft">Loading reservations...</p>
//       </div>
//     );
//   }

//   if (errorList || errorMessage) {
//     return (
//       <div className="mx-auto max-w-3xl px-4 py-10">
//         <ErrorAlert message={errorMessage} list={errorList} />
//       </div>
//     );
//   }

//   if (upcoming.length === 0 && past.length === 0) {
//     return (
//       <div className="mx-auto max-w-3xl px-4 py-10">
//         <div className="mb-8 flex flex-col gap-1">
//           <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Your bookings</p>
//           <h1 className="font-display text-2xl font-semibold text-ink">My Reservations</h1>
//         </div>
//         <EmptyState
//           title="No reservations yet"
//           description="Browse the catalog and book your first event."
//           action={
//             <Link to="/">
//               <Button variant="secondary" className="mt-2">
//                 Browse events
//               </Button>
//             </Link>
//           }
//         />
//       </div>
//     );
//   }

//   return (
//     <div className="mx-auto max-w-3xl px-4 py-10">
//       <div className="mb-8 flex flex-col gap-1">
//         <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Your bookings</p>
//         <h1 className="font-display text-2xl font-semibold text-ink">My Reservations</h1>
//       </div>

//       <section className="mb-10">
//         <h2 className="mb-4 font-display text-base font-semibold text-ink">Upcoming</h2>
//         {upcoming.length === 0 ? (
//           <p className="text-sm text-ink-soft">No upcoming reservations.</p>
//         ) : (
//           <ul className="flex flex-col gap-3">
//             {upcoming.map((reservation) => (
//               <ReservationRow
//                 key={reservation.id}
//                 reservation={reservation}
//                 showCancelButton
//                 isCanceling={cancelingId === reservation.id}
//                 onCancel={() => handleCancel(reservation.id)}
//               />
//             ))}
//           </ul>
//         )}
//       </section>

//       <section>
//         <h2 className="mb-4 font-display text-base font-semibold text-ink">Past</h2>
//         {past.length === 0 ? (
//           <p className="text-sm text-ink-soft">No past reservations.</p>
//         ) : (
//           <ul className="flex flex-col gap-3">
//             {past.map((reservation) => (
//               <ReservationRow key={reservation.id} reservation={reservation} />
//             ))}
//           </ul>
//         )}
//       </section>
//     </div>
//   );
// }

// function ReservationRow({ reservation, showCancelButton = false, isCanceling = false, onCancel }) {
//   const { eventId, eventTitle, eventStartDateTime, eventLocation, numberOfSeats, status } = reservation;

//   return (
//     <li className="flex flex-col gap-3 rounded-xl border border-line bg-paper p-4 sm:flex-row sm:items-center sm:justify-between">
//       <div className="flex flex-col gap-1">
//         <div className="flex items-center gap-2">
//           <Link to={`/events/${eventId}`} className="text-sm font-semibold text-ink hover:text-primary">
//             {eventTitle || 'Deleted event'}
//           </Link>
//           <StatusBadge status={status} />
//         </div>
//         <p className="font-mono text-xs text-ink-soft">{format(parseISO(eventStartDateTime), 'PPp')}</p>
//         <p className="text-sm text-ink-soft">{eventLocation}</p>
//         <p className="text-sm text-ink-soft">{numberOfSeats} seat(s)</p>
//       </div>

//       {showCancelButton && (
//         <Button variant="danger" onClick={onCancel} disabled={isCanceling} className="w-fit">
//           {isCanceling ? 'Cancelling...' : 'Cancel'}
//         </Button>
//       )}
//     </li>
//   );
// }


import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import { Calendar, MapPin, Ticket, Users } from 'lucide-react';
import * as reservationsApi from '../api/reservationsApi';
import { ApiError } from '../api/client';
import StatusBadge from '../components/ui/StatusBadge';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';
import EmptyState from '../components/ui/EmptyState';
import { useToast } from '../context/ToastContext';

export default function DashboardPage() {
  const [reservations, setReservations] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);
  const [cancelingId, setCancelingId] = useState(null);
  const { showToast } = useToast();

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
      showToast('Reservation cancelled.');
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
      <div className="mx-auto max-w-4xl px-4 py-10">
        <DashboardHeader />
        <section className="mb-10">
          <SectionTitle title="Upcoming" count={0} />
          <div className="flex flex-col gap-3">
            <SkeletonRow />
            <SkeletonRow />
          </div>
        </section>
        <section>
          <SectionTitle title="Past" count={0} />
          <div className="flex flex-col gap-3">
            <SkeletonRow />
            <SkeletonRow />
          </div>
        </section>
      </div>
    );
  }

  if (errorList || errorMessage) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <DashboardHeader />
        <ErrorAlert message={errorMessage} list={errorList} />
      </div>
    );
  }

  if (upcoming.length === 0 && past.length === 0) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <DashboardHeader />
        <EmptyState
          title="No reservations yet"
          description="Browse the catalog and book your first event."
          action={
            <Link to="/">
              <Button variant="secondary" className="mt-2">
                Browse events
              </Button>
            </Link>
          }
        />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <DashboardHeader />

      <section className="mb-10">
        <SectionTitle title="Upcoming" count={upcoming.length} />
        {upcoming.length === 0 ? (
          <EmptySectionPlaceholder message="No upcoming reservations." />
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
        <SectionTitle title="Past" count={past.length} />
        {past.length === 0 ? (
          <EmptySectionPlaceholder message="No past reservations." />
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

function DashboardHeader() {
  return (
    <div className="mb-8 flex flex-col gap-1">
      <div className="flex items-center gap-2">
        <span className="flex h-6 w-6 items-center justify-center rounded-md bg-primary/10 text-primary">
          <Ticket className="h-3.5 w-3.5" strokeWidth={2.5} />
        </span>
        <p className="font-mono text-xs font-semibold uppercase tracking-[0.12em] text-ink-soft">
          Your bookings
        </p>
      </div>
      <h1 className="font-display text-2xl font-semibold tracking-tight text-ink">
        My Reservations
      </h1>
    </div>
  );
}

function SectionTitle({ title, count }) {
  return (
    <div className="mb-4 flex items-center justify-between">
      <h2 className="font-display text-base font-semibold text-ink">{title}</h2>
      <span className="rounded-full border border-line bg-surface px-2.5 py-0.5 font-mono text-[10px] font-semibold uppercase tracking-wide text-ink-soft">
        {count}
      </span>
    </div>
  );
}

function EmptySectionPlaceholder({ message }) {
  return (
    <div className="rounded-2xl border border-dashed border-line bg-surface/50 p-8 text-center">
      <p className="text-sm text-ink-soft">{message}</p>
    </div>
  );
}

function SkeletonRow() {
  return (
    <div className="flex flex-col gap-3 rounded-2xl border border-line bg-paper p-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex flex-col gap-2">
        <div className="h-4 w-40 animate-pulse rounded bg-muted" />
        <div className="h-3 w-28 animate-pulse rounded bg-muted" />
        <div className="h-3 w-48 animate-pulse rounded bg-muted" />
      </div>
      <div className="h-9 w-24 animate-pulse rounded-md bg-muted" />
    </div>
  );
}

function ReservationRow({ reservation, showCancelButton = false, isCanceling = false, onCancel }) {
  const { eventId, eventTitle, eventStartDateTime, eventLocation, numberOfSeats, status } = reservation;

  return (
    <li className="group flex flex-col gap-4 rounded-2xl border border-line bg-paper p-5 transition-all duration-200 hover:border-primary/20 hover:bg-surface/40 hover:shadow-sm sm:flex-row sm:items-center sm:justify-between">
      <div className="flex flex-col gap-2.5">
        <div className="flex flex-wrap items-center gap-2">
          <Link
            to={`/events/${eventId}`}
            className="text-base font-semibold text-ink transition-colors hover:text-primary"
          >
            {eventTitle || 'Deleted event'}
          </Link>
          <StatusBadge status={status} />
        </div>

        <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5">
          <div className="flex items-center gap-1.5 text-xs text-ink-soft">
            <Calendar className="h-3.5 w-3.5 text-ink-faint" strokeWidth={2} />
            <span>{format(parseISO(eventStartDateTime), 'PPp')}</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs text-ink-soft">
            <MapPin className="h-3.5 w-3.5 text-ink-faint" strokeWidth={2} />
            <span>{eventLocation}</span>
          </div>
          <div className="flex items-center gap-1.5 text-xs text-ink-soft">
            <Users className="h-3.5 w-3.5 text-ink-faint" strokeWidth={2} />
            <span>
              {numberOfSeats} seat{numberOfSeats === 1 ? '' : 's'}
            </span>
          </div>
        </div>
      </div>

      {showCancelButton && (
        <Button
          variant="danger"
          onClick={onCancel}
          disabled={isCanceling}
          className="w-full sm:w-fit"
        >
          {isCanceling ? 'Cancelling...' : 'Cancel'}
        </Button>
      )}
    </li>
  );
}
