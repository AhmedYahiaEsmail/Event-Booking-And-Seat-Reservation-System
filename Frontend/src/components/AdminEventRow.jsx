import { format, parseISO } from 'date-fns';

// Kept as its own component (rather than inlined in AdminDashboardPage) because a
// future step adds an Edit action to these same rows — this keeps that addition
// isolated to one file instead of reworking the table markup in the page.
export default function AdminEventRow({ event, isActioning, onPublish, onCancel, onComplete, onEdit }) {
  const { id, title, status, startDateTime, availableSeats, totalSeats } = event;
  const isEditable = status === 'Draft' || status === 'Published';

  return (
    <tr className="border-b border-gray-200">
      <td className="px-3 py-2 text-sm text-gray-900">{title}</td>
      <td className="px-3 py-2 text-sm text-gray-600">{status}</td>
      <td className="px-3 py-2 text-sm text-gray-600">{format(parseISO(startDateTime), 'PPp')}</td>
      <td className="px-3 py-2 text-sm text-gray-600">
        {availableSeats} / {totalSeats}
      </td>
      <td className="px-3 py-2">
        <div className="flex gap-2">
          {status === 'Draft' && (
            <button
              type="button"
              onClick={() => onPublish(id)}
              disabled={isActioning}
              className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isActioning ? 'Publishing...' : 'Publish'}
            </button>
          )}

          {status === 'Published' && (
            <>
              <button
                type="button"
                onClick={() => onCancel(id)}
                disabled={isActioning}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isActioning ? 'Cancelling...' : 'Cancel'}
              </button>
              <button
                type="button"
                onClick={() => onComplete(id)}
                disabled={isActioning}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isActioning ? 'Completing...' : 'Complete'}
              </button>
            </>
          )}

          {isEditable && (
            <button
              type="button"
              onClick={() => onEdit(event)}
              disabled={isActioning}
              className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Edit
            </button>
          )}
        </div>
      </td>
    </tr>
  );
}
