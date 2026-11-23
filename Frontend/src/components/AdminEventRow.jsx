// import { format, parseISO } from 'date-fns';
// import StatusBadge from './ui/StatusBadge';
// import Button from './ui/Button';

// export default function AdminEventRow({ event, isActioning, onPublish, onCancel, onComplete, onEdit }) {
//   const { id, title, status, startDateTime, availableSeats, totalSeats } = event;
//   const isEditable = status === 'Draft' || status === 'Published';

//   return (
//     <tr className="border-b border-line last:border-0">
//       <td className="px-3 py-3 text-sm font-medium text-ink">{title}</td>
//       <td className="px-3 py-3">
//         <StatusBadge status={status} />
//       </td>
//       <td className="px-3 py-3 font-mono text-xs text-ink-soft">
//         {format(parseISO(startDateTime), 'PPp')}
//       </td>
//       <td className="px-3 py-3 font-mono text-xs tabular-nums text-ink-soft">
//         {availableSeats}/{totalSeats}
//       </td>
//       <td className="px-3 py-3">
//         <div className="flex flex-wrap gap-2">
//           {status === 'Draft' && (
//             <Button variant="primary" onClick={() => onPublish(id)} disabled={isActioning} className="px-3 py-1.5 text-xs">
//               {isActioning ? 'Publishing...' : 'Publish'}
//             </Button>
//           )}

//           {status === 'Published' && (
//             <>
//               <Button variant="secondary" onClick={() => onCancel(id)} disabled={isActioning} className="px-3 py-1.5 text-xs">
//                 {isActioning ? 'Cancelling...' : 'Cancel'}
//               </Button>
//               <Button variant="secondary" onClick={() => onComplete(id)} disabled={isActioning} className="px-3 py-1.5 text-xs">
//                 {isActioning ? 'Completing...' : 'Complete'}
//               </Button>
//             </>
//           )}

//           {isEditable && (
//             <Button variant="ghost" onClick={() => onEdit(event)} disabled={isActioning} className="px-3 py-1.5 text-xs">
//               Edit
//             </Button>
//           )}
//         </div>
//       </td>
//     </tr>
//   );
// }



import { format, parseISO } from 'date-fns';
import StatusBadge from './ui/StatusBadge';
import Button from './ui/Button';

export default function AdminEventRow({ event, isActioning, onPublish, onCancel, onComplete, onEdit }) {
  const { id, title, status, startDateTime, availableSeats, totalSeats } = event;
  const isEditable = status === 'Draft' || status === 'Published';

  return (
    <tr className="border-b border-line last:border-0">
      <td className="px-3 py-3 text-sm font-medium text-ink">{title}</td>
      <td className="px-3 py-3">
        <StatusBadge status={status} />
      </td>
      <td className="px-3 py-3 font-mono text-xs text-ink-soft">
        {format(parseISO(startDateTime), 'PPp')}
      </td>
      <td className="px-3 py-3 font-mono text-xs tabular-nums text-ink-soft">
        {availableSeats}/{totalSeats}
      </td>
      <td className="px-3 py-3">
        <div className="flex flex-wrap gap-2">
          {status === 'Draft' && (
            <Button 
              variant="primary"
              onClick={() => onPublish(id)} 
              disabled={isActioning} 
              className="px-3 py-1.5 text-xs font-semibold"
            >
              {isActioning ? 'Publishing...' : 'Publish'}
            </Button>
          )}

          {status === 'Published' && (
            <>
              <Button 
                variant="secondary"
                onClick={() => onCancel(id)} 
                disabled={isActioning} 
                className="!border-red-200 !bg-red-50 !text-red-700 hover:!bg-red-100 px-3 py-1.5 text-xs font-semibold"
              >
                {isActioning ? 'Cancelling...' : 'Cancel'}
              </Button>

              <Button 
                variant="secondary"
                onClick={() => onComplete(id)} 
                disabled={isActioning} 
                className="!border-emerald-200 !bg-emerald-50 !text-emerald-700 hover:!bg-emerald-100 px-3 py-1.5 text-xs font-semibold"
              >
                {isActioning ? 'Completing...' : 'Complete'}
              </Button>
            </>
          )}

          {isEditable && (
            <Button 
              variant="secondary"
              onClick={() => onEdit(event)} 
              disabled={isActioning} 
              className="px-3 py-1.5 text-xs font-semibold"
            >
              Edit
            </Button>
          )}
        </div>
      </td>
    </tr>
  );
}