import { Link } from 'react-router-dom';
import { format, parseISO } from 'date-fns';

export default function EventCard({ event }) {
  const isSoldOut = event.availableSeats === 0;

  const dateRange = `${format(parseISO(event.startDateTime), 'PPp')} – ${format(
    parseISO(event.endDateTime),
    'PPp'
  )}`;

  return (
    <Link
      to={`/events/${event.id}`}
      className="flex flex-col gap-2 rounded-md border border-gray-200 p-4 transition hover:border-gray-400 hover:shadow-sm"
    >
      <div className="flex items-start justify-between gap-2">
        <h2 className="text-base font-semibold text-gray-900">{event.title}</h2>
        {isSoldOut && (
          <span className="shrink-0 rounded-md bg-gray-900 px-2 py-0.5 text-xs font-medium text-white">
            Sold out
          </span>
        )}
      </div>

      <p className="text-sm text-gray-600">{dateRange}</p>
      <p className="text-sm text-gray-600">{event.location}</p>
      <p className="text-sm text-gray-600">{event.speakerName}</p>

      <p className="mt-1 text-sm font-medium text-gray-700">
        {event.availableSeats} / {event.totalSeats} seats available
      </p>
    </Link>
  );
}
