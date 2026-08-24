import { Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';
import StatusBadge from './ui/StatusBadge';
import SeatGauge from './ui/SeatGauge';

export default function EventCard({ event }) {
  const isSoldOut = event.availableSeats === 0;

  return (
    <Link
      to={`/events/${event.id}`}
      className="group relative flex flex-col overflow-hidden rounded-2xl border border-line bg-paper shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="flex flex-col gap-2 p-5">
        <div className="flex items-start justify-between gap-2">
          <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">
            {format(parseISO(event.startDateTime), 'MMM d, yyyy')}
          </p>
          {isSoldOut && <StatusBadge status="Cancelled" statusLabel="Sold out" />}
        </div>

        <h2 className="font-display text-lg font-semibold leading-snug text-ink group-hover:text-primary">
          {event.title}
        </h2>

        <p className="text-sm text-ink-soft">{event.location}</p>
        <p className="text-sm text-ink-soft">{event.speakerName}</p>
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