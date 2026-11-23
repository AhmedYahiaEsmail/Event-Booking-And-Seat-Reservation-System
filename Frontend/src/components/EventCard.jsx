// import { Link } from 'react-router-dom';
// import { format, parseISO } from 'date-fns';
// import StatusBadge from './ui/StatusBadge';
// import SeatGauge from './ui/SeatGauge';

// export default function EventCard({ event }) {
//   const isSoldOut = event.availableSeats === 0;

//   return (
//     <Link
//       to={`/events/${event.id}`}
//       className="group relative flex flex-col overflow-hidden rounded-2xl border border-line bg-paper shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
//     >
//       <div className="flex flex-col gap-2 p-5">
//         <div className="flex items-start justify-between gap-2">
//           <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">
//             {format(parseISO(event.startDateTime), 'MMM d, yyyy')}
//           </p>
//           {isSoldOut && <StatusBadge status="Cancelled" statusLabel="Sold out" />}
//         </div>

//         <h2 className="font-display text-lg font-semibold leading-snug text-ink group-hover:text-primary">
//           {event.title}
//         </h2>

//         <p className="text-sm text-ink-soft">{event.location}</p>
//         <p className="text-sm text-ink-soft">{event.speakerName}</p>
//       </div>

//       {/* Ticket perforation divider */}
//       <div className="relative border-t border-dashed border-line">
//         <span className="absolute -left-3 -top-3 h-6 w-6 rounded-full bg-surface" />
//         <span className="absolute -right-3 -top-3 h-6 w-6 rounded-full bg-surface" />
//       </div>

//       <div className="p-5 pt-4">
//         <SeatGauge available={event.availableSeats} total={event.totalSeats} />
//       </div>
//     </Link>
//   );
// }

import { Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import { MapPin, User, Calendar } from 'lucide-react';
import StatusBadge from './ui/StatusBadge';
import SeatGauge from './ui/SeatGauge';

export default function EventCard({ event }) {
  const isSoldOut = event.availableSeats === 0;

  return (
    <Link
      to={`/events/${event.id}`}
      className="group relative flex flex-col overflow-hidden rounded-2xl border border-line bg-paper shadow-sm transition-all duration-300 hover:-translate-y-1.5 hover:shadow-xl"
    >
      {/* Image cover */}
      <div className="relative h-44 w-full overflow-hidden">
        <img
          src={event.imageUrl || 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800&q=80'}
          alt={event.title}
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
        />

{/* Date badge */}
<div className="absolute left-3 top-3 flex items-center gap-1.5 rounded-xl border border-line bg-paper/95 px-3 py-1.5 shadow-md backdrop-blur-md">
  <Calendar size={14} className="shrink-0 text-primary" />
  <span className="font-mono text-xs font-bold uppercase tracking-wider text-ink">
    {format(parseISO(event.startDateTime), 'MMM d, yyyy')}
  </span>
</div>

        {/* Sold out badge */}
        {isSoldOut && (
          <div className="absolute right-4 top-4">
            <StatusBadge status="Cancelled" statusLabel="Sold out" />
          </div>
        )}
      </div>

      <div className="flex flex-col gap-3 p-5">
        <h2 className="font-display text-lg font-semibold leading-snug text-ink group-hover:text-primary">
          {event.title}
        </h2>

        <div className="flex items-center gap-2 text-sm text-ink-soft">
          <MapPin size={16} className="shrink-0 text-ink-faint" />
          <span className="truncate">{event.location}</span>
        </div>

        <div className="flex items-center gap-2 text-sm text-ink-soft">
          <User size={16} className="shrink-0 text-ink-faint" />
          <span className="truncate">{event.speakerName}</span>
        </div>
      </div>

      {/* Ticket perforation divider */}
      <div className="relative border-t border-dashed border-line">
        <span className="absolute -left-3 -top-3 h-6 w-6 rounded-full bg-surface" />
        <span className="absolute -right-3 -top-3 h-6 w-6 rounded-full bg-surface" />
      </div>

      <div className="p-5 pt-4">
        <SeatGauge available={event.availableSeats} total={event.totalSeats} />
      </div>
    </Link>
  );
}
