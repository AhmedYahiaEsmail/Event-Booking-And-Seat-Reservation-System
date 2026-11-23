// import { useEffect, useState } from 'react';
// import { format, parseISO } from 'date-fns';
// import * as eventsApi from '../api/eventsApi';
// import { ApiError } from '../api/client';
// import AdminEventRow from '../components/AdminEventRow';
// import Input from '../components/ui/Input';
// import Textarea from '../components/ui/Textarea';
// import Button from '../components/ui/Button';
// import { ErrorAlert } from '../components/ui/Alert';
// import EmptyState from '../components/ui/EmptyState';
// import { useToast } from '../context/ToastContext';

// const EMPTY_FORM = {
//   title: '',
//   description: '',
//   startDateTime: '',
//   endDateTime: '',
//   location: '',
//   speakerName: '',
//   speakerBio: '',
//   totalSeats: 1,
// };

// export default function AdminDashboardPage() {
//   const [events, setEvents] = useState([]);
//   const [isLoading, setIsLoading] = useState(true);
//   const [errorMessage, setErrorMessage] = useState(null);
//   const [errorList, setErrorList] = useState(null);
//   const [refetchTrigger, setRefetchTrigger] = useState(0);
//   const [actioningId, setActioningId] = useState(null);

//   const [formValues, setFormValues] = useState(EMPTY_FORM);
//   const [isSubmitting, setIsSubmitting] = useState(false);
//   const [formErrorMessage, setFormErrorMessage] = useState(null);
//   const [formErrorList, setFormErrorList] = useState(null);
//   const [editingEvent, setEditingEvent] = useState(null);

//   const { showToast } = useToast();

//   useEffect(() => {
//     let isCancelled = false;

//     async function fetchEvents() {
//       setIsLoading(true);
//       setErrorMessage(null);
//       setErrorList(null);

//       try {
//         const data = await eventsApi.getEvents({ pageSize: 100 });
//         if (!isCancelled) {
//           setEvents(data ?? []);
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

//     fetchEvents();

//     return () => {
//       isCancelled = true;
//     };
//   }, [refetchTrigger]);

//   function handleFormChange(field, value) {
//     setFormValues((current) => ({ ...current, [field]: value }));
//   }

//   function handleEditClick(event) {
//     setEditingEvent(event);
//     setFormErrorMessage(null);
//     setFormErrorList(null);
//     setFormValues({
//       title: event.title,
//       description: event.description ?? '',
//       startDateTime: format(parseISO(event.startDateTime), "yyyy-MM-dd'T'HH:mm"),
//       endDateTime: format(parseISO(event.endDateTime), "yyyy-MM-dd'T'HH:mm"),
//       location: event.location,
//       speakerName: event.speakerName,
//       speakerBio: event.speakerBio ?? '',
//       totalSeats: event.totalSeats,
//     });
//   }

//   function handleCancelEdit() {
//     setEditingEvent(null);
//     setFormValues(EMPTY_FORM);
//     setFormErrorMessage(null);
//     setFormErrorList(null);
//   }

//   async function handleFormSubmit(event) {
//     event.preventDefault();
//     setFormErrorMessage(null);
//     setFormErrorList(null);
//     setIsSubmitting(true);

//     const payload = {
//       title: formValues.title,
//       description: formValues.description.trim() || undefined,
//       startDateTime: new Date(formValues.startDateTime).toISOString(),
//       endDateTime: new Date(formValues.endDateTime).toISOString(),
//       location: formValues.location,
//       speakerName: formValues.speakerName,
//       speakerBio: formValues.speakerBio.trim() || undefined,
//       totalSeats: Number(formValues.totalSeats),
//     };

//     try {
//       if (editingEvent) {
//         await eventsApi.updateEvent(editingEvent.id, { ...payload, rowVersion: editingEvent.rowVersion });
//         showToast('Event updated.');
//       } else {
//         await eventsApi.createEvent(payload);
//         showToast('Event created.');
//       }

//       setEditingEvent(null);
//       setFormValues(EMPTY_FORM);
//       setRefetchTrigger((current) => current + 1);
//     } catch (error) {
//       if (error instanceof ApiError && error.status === 409) {
//         setFormErrorMessage(error.message);
//         setEditingEvent(null);
//         setFormValues(EMPTY_FORM);
//         setRefetchTrigger((current) => current + 1);
//       } else if (error instanceof ApiError) {
//         if (Array.isArray(error.errors) && error.errors.length > 0) {
//           setFormErrorList(error.errors);
//         } else {
//           setFormErrorMessage(error.message);
//         }
//       } else {
//         setFormErrorMessage('Something went wrong. Please try again.');
//       }
//     } finally {
//       setIsSubmitting(false);
//     }
//   }

//   async function runLifecycleAction(id, action) {
//     setActioningId(id);
//     setErrorMessage(null);
//     setErrorList(null);

//     try {
//       await action(id);
//       showToast('Done.');
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
//       setActioningId(null);
//     }
//   }

//   const handlePublish = (id) => runLifecycleAction(id, eventsApi.publishEvent);
//   const handleCancel = (id) => runLifecycleAction(id, eventsApi.cancelEvent);
//   const handleComplete = (id) => runLifecycleAction(id, eventsApi.completeEvent);

//   return (
//     <div className="mx-auto max-w-5xl px-4 py-10">
//       <div className="mb-8 flex flex-col gap-1">
//         <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Admin</p>
//         <h1 className="font-display text-2xl font-semibold text-ink">Event Management</h1>
//       </div>

//       <section className="mb-10 overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
//         <div className="border-b border-line bg-surface px-6 py-4">
//           <h2 className="font-display text-base font-semibold text-ink">
//             {editingEvent ? `Edit “${editingEvent.title}”` : 'Create a new event'}
//           </h2>
//         </div>

//         <form onSubmit={handleFormSubmit} className="flex flex-col gap-4 p-6">
//           <Input
//             label="Title"
//             id="title"
//             type="text"
//             value={formValues.title}
//             onChange={(e) => handleFormChange('title', e.target.value)}
//             required
//           />

//           <Textarea
//             label="Description"
//             id="description"
//             value={formValues.description}
//             onChange={(e) => handleFormChange('description', e.target.value)}
//           />

//           <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
//             <Input
//               label="Start"
//               id="startDateTime"
//               type="datetime-local"
//               value={formValues.startDateTime}
//               onChange={(e) => handleFormChange('startDateTime', e.target.value)}
//               required
//             />
//             <Input
//               label="End"
//               id="endDateTime"
//               type="datetime-local"
//               value={formValues.endDateTime}
//               onChange={(e) => handleFormChange('endDateTime', e.target.value)}
//               required
//             />
//           </div>

//           <Input
//             label="Location"
//             id="location"
//             type="text"
//             value={formValues.location}
//             onChange={(e) => handleFormChange('location', e.target.value)}
//             required
//           />

//           <Input
//             label="Speaker name"
//             id="speakerName"
//             type="text"
//             value={formValues.speakerName}
//             onChange={(e) => handleFormChange('speakerName', e.target.value)}
//             required
//           />

//           <Textarea
//             label="Speaker bio"
//             id="speakerBio"
//             value={formValues.speakerBio}
//             onChange={(e) => handleFormChange('speakerBio', e.target.value)}
//           />

//           <div className="w-40">
//             <Input
//               label="Total seats"
//               id="totalSeats"
//               type="number"
//               min={1}
//               value={formValues.totalSeats}
//               onChange={(e) => handleFormChange('totalSeats', e.target.value)}
//               required
//             />
//           </div>

//           <ErrorAlert message={formErrorMessage} list={formErrorList} />

//           <div className="flex gap-2 pt-1">
//             <Button type="submit" disabled={isSubmitting} className="w-fit">
//               {isSubmitting
//                 ? editingEvent
//                   ? 'Saving...'
//                   : 'Creating...'
//                 : editingEvent
//                   ? 'Save changes'
//                   : 'Create event'}
//             </Button>

//             {editingEvent && (
//               <Button type="button" variant="secondary" onClick={handleCancelEdit} className="w-fit">
//                 Cancel
//               </Button>
//             )}
//           </div>
//         </form>
//       </section>

//       <section>
//         <h2 className="mb-4 font-display text-base font-semibold text-ink">All Events</h2>

//         {isLoading && <p className="text-sm text-ink-soft">Loading events...</p>}

//         {!isLoading && (errorList || errorMessage) && <ErrorAlert message={errorMessage} list={errorList} />}

//         {!isLoading && !errorMessage && !errorList && events.length === 0 && (
//           <EmptyState title="No events yet" description="Create your first event using the form above." />
//         )}

//         {!isLoading && !errorMessage && !errorList && events.length > 0 && (
//           <div className="overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
//             <div className="overflow-x-auto">
//               <table className="w-full border-collapse text-left">
//                 <thead>
//                   <tr className="border-b border-line bg-surface">
//                     <th className="px-3 py-3 text-xs font-semibold uppercase tracking-wide text-ink-soft">Title</th>
//                     <th className="px-3 py-3 text-xs font-semibold uppercase tracking-wide text-ink-soft">Status</th>
//                     <th className="px-3 py-3 text-xs font-semibold uppercase tracking-wide text-ink-soft">Start</th>
//                     <th className="px-3 py-3 text-xs font-semibold uppercase tracking-wide text-ink-soft">Seats</th>
//                     <th className="px-3 py-3 text-xs font-semibold uppercase tracking-wide text-ink-soft">Actions</th>
//                   </tr>
//                 </thead>
//                 <tbody>
//                   {events.map((event) => (
//                     <AdminEventRow
//                       key={event.id}
//                       event={event}
//                       isActioning={actioningId === event.id}
//                       onPublish={handlePublish}
//                       onCancel={handleCancel}
//                       onComplete={handleComplete}
//                       onEdit={handleEditClick}
//                     />
//                   ))}
//                 </tbody>
//               </table>
//             </div>
//           </div>
//         )}
//       </section>
//     </div>
//   );
// }

import { useEffect, useState } from 'react';
import { format, parseISO } from 'date-fns';
import {
  ShieldCheck,
  Calendar,
  Loader2,
  Pencil,
  Plus,
  X,
} from 'lucide-react';
import * as eventsApi from '../api/eventsApi';
import { ApiError } from '../api/client';
import AdminEventRow from '../components/AdminEventRow';
import Input from '../components/ui/Input';
import Textarea from '../components/ui/Textarea';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';
import EmptyState from '../components/ui/EmptyState';
import { useToast } from '../context/ToastContext';

const EMPTY_FORM = {
  title: '',
  description: '',
  startDateTime: '',
  endDateTime: '',
  location: '',
  speakerName: '',
  speakerBio: '',
  totalSeats: 1,
};

export default function AdminDashboardPage() {
  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);
  const [refetchTrigger, setRefetchTrigger] = useState(0);
  const [actioningId, setActioningId] = useState(null);

  const [formValues, setFormValues] = useState(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formErrorMessage, setFormErrorMessage] = useState(null);
  const [formErrorList, setFormErrorList] = useState(null);
  const [editingEvent, setEditingEvent] = useState(null);

  const { showToast } = useToast();

  useEffect(() => {
    let isCancelled = false;

    async function fetchEvents() {
      setIsLoading(true);
      setErrorMessage(null);
      setErrorList(null);

      try {
        const data = await eventsApi.getEvents({ pageSize: 100 });
        if (!isCancelled) {
          setEvents(data ?? []);
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

    fetchEvents();

    return () => {
      isCancelled = true;
    };
  }, [refetchTrigger]);

  function handleFormChange(field, value) {
    setFormValues((current) => ({ ...current, [field]: value }));
  }

  function handleEditClick(event) {
    setEditingEvent(event);
    setFormErrorMessage(null);
    setFormErrorList(null);
    setFormValues({
      title: event.title,
      description: event.description ?? '',
      startDateTime: format(parseISO(event.startDateTime), "yyyy-MM-dd'T'HH:mm"),
      endDateTime: format(parseISO(event.endDateTime), "yyyy-MM-dd'T'HH:mm"),
      location: event.location,
      speakerName: event.speakerName,
      speakerBio: event.speakerBio ?? '',
      totalSeats: event.totalSeats,
    });

    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function handleCancelEdit() {
    setEditingEvent(null);
    setFormValues(EMPTY_FORM);
    setFormErrorMessage(null);
    setFormErrorList(null);
  }

  async function handleFormSubmit(event) {
    event.preventDefault();
    setFormErrorMessage(null);
    setFormErrorList(null);
    setIsSubmitting(true);

    const payload = {
      title: formValues.title,
      description: formValues.description.trim() || undefined,
      startDateTime: new Date(formValues.startDateTime).toISOString(),
      endDateTime: new Date(formValues.endDateTime).toISOString(),
      location: formValues.location,
      speakerName: formValues.speakerName,
      speakerBio: formValues.speakerBio.trim() || undefined,
      totalSeats: Number(formValues.totalSeats),
    };

    try {
      if (editingEvent) {
        await eventsApi.updateEvent(editingEvent.id, { ...payload, rowVersion: editingEvent.rowVersion });
        showToast('Event updated.');
      } else {
        await eventsApi.createEvent(payload);
        showToast('Event created.');
      }

      setEditingEvent(null);
      setFormValues(EMPTY_FORM);
      setRefetchTrigger((current) => current + 1);
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) {
        setFormErrorMessage(error.message);
        setEditingEvent(null);
        setFormValues(EMPTY_FORM);
        setRefetchTrigger((current) => current + 1);
      } else if (error instanceof ApiError) {
        if (Array.isArray(error.errors) && error.errors.length > 0) {
          setFormErrorList(error.errors);
        } else {
          setFormErrorMessage(error.message);
        }
      } else {
        setFormErrorMessage('Something went wrong. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  async function runLifecycleAction(id, action) {
    setActioningId(id);
    setErrorMessage(null);
    setErrorList(null);

    try {
      await action(id);
      showToast('Done.');
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
      setActioningId(null);
    }
  }

  const handlePublish = (id) => runLifecycleAction(id, eventsApi.publishEvent);
  const handleCancel = (id) => runLifecycleAction(id, eventsApi.cancelEvent);
  const handleComplete = (id) => runLifecycleAction(id, eventsApi.completeEvent);

  const formCardBorder = editingEvent
    ? 'border-primary/30 shadow-primary/5'
    : 'border-line';
  const formCardHeader = editingEvent
    ? 'bg-primary/[0.03] border-primary/20'
    : 'bg-surface border-line';

  return (
    <div className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8 flex flex-col gap-1">
        <div className="flex items-center gap-2">
          <span className="flex h-6 w-6 items-center justify-center rounded-md bg-primary/10 text-primary">
            <ShieldCheck className="h-3.5 w-3.5" strokeWidth={2.5} />
          </span>
          <p className="font-mono text-xs font-semibold uppercase tracking-[0.12em] text-ink-soft">
            Admin control center
          </p>
        </div>
        <h1 className="font-display text-2xl font-semibold tracking-tight text-ink">
          Event Management
        </h1>
      </div>

      <section
        className={`mb-10 overflow-hidden rounded-2xl border bg-paper shadow-sm transition-colors ${formCardBorder}`}
      >
        <div
          className={`flex items-center justify-between border-b px-6 py-4 ${formCardHeader}`}
        >
          <div className="flex items-center gap-2">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-accent text-accent-foreground">
              {editingEvent ? (
                <Pencil className="h-4 w-4" strokeWidth={2} />
              ) : (
                <Plus className="h-4 w-4" strokeWidth={2} />
              )}
            </span>
            <h2 className="font-display text-base font-semibold text-ink">
              {editingEvent
                ? `Edit “${editingEvent.title}”`
                : 'Create a new event'}
            </h2>
          </div>
          {editingEvent && (
            <span className="rounded-full border border-primary/20 bg-primary/10 px-2.5 py-0.5 font-mono text-[10px] font-semibold uppercase tracking-wide text-primary">
              Editing
            </span>
          )}
        </div>

        <form onSubmit={handleFormSubmit} className="flex flex-col gap-4 p-6">
          <Input
            label="Title"
            id="title"
            type="text"
            value={formValues.title}
            onChange={(e) => handleFormChange('title', e.target.value)}
            required
          />

          <Textarea
            label="Description"
            id="description"
            value={formValues.description}
            onChange={(e) => handleFormChange('description', e.target.value)}
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Input
              label="Start"
              id="startDateTime"
              type="datetime-local"
              value={formValues.startDateTime}
              onChange={(e) => handleFormChange('startDateTime', e.target.value)}
              required
            />
            <Input
              label="End"
              id="endDateTime"
              type="datetime-local"
              value={formValues.endDateTime}
              onChange={(e) => handleFormChange('endDateTime', e.target.value)}
              required
            />
          </div>

          <Input
            label="Location"
            id="location"
            type="text"
            value={formValues.location}
            onChange={(e) => handleFormChange('location', e.target.value)}
            required
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-[1fr_140px]">
            <Input
              label="Speaker name"
              id="speakerName"
              type="text"
              value={formValues.speakerName}
              onChange={(e) => handleFormChange('speakerName', e.target.value)}
              required
            />
            <Input
              label="Total seats"
              id="totalSeats"
              type="number"
              min={1}
              value={formValues.totalSeats}
              onChange={(e) => handleFormChange('totalSeats', e.target.value)}
              required
            />
          </div>

          <Textarea
            label="Speaker bio"
            id="speakerBio"
            value={formValues.speakerBio}
            onChange={(e) => handleFormChange('speakerBio', e.target.value)}
          />

          <ErrorAlert message={formErrorMessage} list={formErrorList} />

          <div className="flex flex-wrap items-center gap-2 pt-1">
            <Button type="submit" disabled={isSubmitting} className="w-fit">
              {isSubmitting && (
                <Loader2 className="mr-1.5 h-4 w-4 animate-spin" />
              )}
              {isSubmitting
                ? editingEvent
                  ? 'Saving...'
                  : 'Creating...'
                : editingEvent
                  ? 'Save changes'
                  : 'Create event'}
            </Button>

            {editingEvent && (
              <Button
                type="button"
                variant="secondary"
                onClick={handleCancelEdit}
                className="w-fit"
              >
                <X className="mr-1.5 h-4 w-4" />
                Cancel
              </Button>
            )}
          </div>
        </form>
      </section>

      <section>
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-ink-faint" strokeWidth={2} />
            <h2 className="font-display text-base font-semibold text-ink">
              All Events
            </h2>
          </div>
          <span className="rounded-full border border-line bg-surface px-2.5 py-0.5 font-mono text-[10px] font-semibold uppercase tracking-wide text-ink-soft">
            {events.length} total
          </span>
        </div>

        {isLoading && <SkeletonTable />}

        {!isLoading && (errorList || errorMessage) && (
          <ErrorAlert message={errorMessage} list={errorList} />
        )}

        {!isLoading && !errorMessage && !errorList && events.length === 0 && (
          <EmptyState
            title="No events yet"
            description="Create your first event using the form above."
          />
        )}

        {!isLoading && !errorMessage && !errorList && events.length > 0 && (
          <div className="overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-left">
                <thead>
                  <tr className="border-b border-line bg-surface">
                    <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                      Title
                    </th>
                    <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                      Status
                    </th>
                    <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                      Start
                    </th>
                    <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                      Seats
                    </th>
                    <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {events.map((event) => (
                    <AdminEventRow
                      key={event.id}
                      event={event}
                      isActioning={actioningId === event.id}
                      onPublish={handlePublish}
                      onCancel={handleCancel}
                      onComplete={handleComplete}
                      onEdit={handleEditClick}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </section>
    </div>
  );
}

function SkeletonTable() {
  return (
    <div className="overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-left">
          <thead>
            <tr className="border-b border-line bg-surface">
              <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                Title
              </th>
              <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                Status
              </th>
              <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                Start
              </th>
              <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                Seats
              </th>
              <th className="px-4 py-3.5 text-xs font-semibold uppercase tracking-wide text-ink-soft">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {[...Array(5)].map((_, index) => (
              <tr key={index} className="border-b border-line last:border-b-0">
                <td className="px-4 py-4">
                  <div className="h-4 w-40 rounded bg-muted" />
                </td>
                <td className="px-4 py-4">
                  <div className="h-5 w-16 rounded-full bg-muted" />
                </td>
                <td className="px-4 py-4">
                  <div className="h-4 w-28 rounded bg-muted" />
                </td>
                <td className="px-4 py-4">
                  <div className="h-4 w-12 rounded bg-muted" />
                </td>
                <td className="px-4 py-4">
                  <div className="flex gap-2">
                    <div className="h-8 w-16 rounded-md bg-muted" />
                    <div className="h-8 w-16 rounded-md bg-muted" />
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
