import { useEffect, useState } from 'react';
import { format, parseISO } from 'date-fns';
import * as eventsApi from '../api/eventsApi';
import { ApiError } from '../api/client';
import AdminEventRow from '../components/AdminEventRow';

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
  // null => Create mode, set => Edit mode. Deliberately not tracked with a
  // separate isEditing boolean — the presence of an event here is what the
  // form's mode is derived from everywhere below.
  const [editingEvent, setEditingEvent] = useState(null);

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
      // Reverse of the ISO -> datetime-local conversion the Create path does on
      // submit, so the existing dates populate the datetime-local inputs correctly
      // instead of showing raw ISO strings.
      startDateTime: format(parseISO(event.startDateTime), "yyyy-MM-dd'T'HH:mm"),
      endDateTime: format(parseISO(event.endDateTime), "yyyy-MM-dd'T'HH:mm"),
      location: event.location,
      speakerName: event.speakerName,
      speakerBio: event.speakerBio ?? '',
      totalSeats: event.totalSeats,
      // rowVersion is deliberately NOT copied into formValues — it isn't a
      // rendered field. It's read straight from editingEvent.rowVersion when the
      // update payload is built in handleFormSubmit below.
    });
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
      // Omit optional fields entirely when left empty rather than sending
      // empty strings.
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
      } else {
        await eventsApi.createEvent(payload);
      }

      setEditingEvent(null);
      setFormValues(EMPTY_FORM);
      setRefetchTrigger((current) => current + 1);
    } catch (error) {
      // Checked first, ahead of the generic three-way branch: a 409 here means
      // someone else changed this event since it was loaded, not a validation
      // failure. error.message is already the exact backend-authored text, shown
      // as-is. Recovery is reset-to-create-mode + refetch, per spec — no diffing
      // or merge UI, and no attempt to keep the user's in-progress edits, since
      // the rowVersion they submitted against is stale.
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
        // editingEvent/formValues intentionally left untouched here so the
        // Admin can see and fix their input without losing their edits.
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

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="mb-6 text-xl font-semibold text-gray-900">Admin Dashboard</h1>

      <section className="mb-10 rounded-md border border-gray-200 p-4">
        <h2 className="mb-4 text-lg font-semibold text-gray-900">
          {editingEvent ? 'Edit Event' : 'Create Event'}
        </h2>

        <form onSubmit={handleFormSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label htmlFor="title" className="text-sm font-medium text-gray-700">
              Title
            </label>
            <input
              id="title"
              type="text"
              value={formValues.title}
              onChange={(e) => handleFormChange('title', e.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="description" className="text-sm font-medium text-gray-700">
              Description
            </label>
            <textarea
              id="description"
              value={formValues.description}
              onChange={(e) => handleFormChange('description', e.target.value)}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label htmlFor="startDateTime" className="text-sm font-medium text-gray-700">
                Start
              </label>
              <input
                id="startDateTime"
                type="datetime-local"
                value={formValues.startDateTime}
                onChange={(e) => handleFormChange('startDateTime', e.target.value)}
                required
                className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
              />
            </div>

            <div className="flex flex-col gap-1">
              <label htmlFor="endDateTime" className="text-sm font-medium text-gray-700">
                End
              </label>
              <input
                id="endDateTime"
                type="datetime-local"
                value={formValues.endDateTime}
                onChange={(e) => handleFormChange('endDateTime', e.target.value)}
                required
                className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
              />
            </div>
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="location" className="text-sm font-medium text-gray-700">
              Location
            </label>
            <input
              id="location"
              type="text"
              value={formValues.location}
              onChange={(e) => handleFormChange('location', e.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="speakerName" className="text-sm font-medium text-gray-700">
              Speaker name
            </label>
            <input
              id="speakerName"
              type="text"
              value={formValues.speakerName}
              onChange={(e) => handleFormChange('speakerName', e.target.value)}
              required
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="speakerBio" className="text-sm font-medium text-gray-700">
              Speaker bio
            </label>
            <textarea
              id="speakerBio"
              value={formValues.speakerBio}
              onChange={(e) => handleFormChange('speakerBio', e.target.value)}
              className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="totalSeats" className="text-sm font-medium text-gray-700">
              Total seats
            </label>
            <input
              id="totalSeats"
              type="number"
              min={1}
              value={formValues.totalSeats}
              onChange={(e) => handleFormChange('totalSeats', e.target.value)}
              required
              className="w-32 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
            />
          </div>

          {formErrorList && (
            <ul className="list-inside list-disc text-sm text-red-600">
              {formErrorList.map((message) => (
                <li key={message}>{message}</li>
              ))}
            </ul>
          )}

          {formErrorMessage && <p className="text-sm text-red-600">{formErrorMessage}</p>}

          <div className="flex gap-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-fit rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting
                ? editingEvent
                  ? 'Saving...'
                  : 'Creating...'
                : editingEvent
                  ? 'Save Changes'
                  : 'Create Event'}
            </button>

            {editingEvent && (
              <button
                type="button"
                onClick={handleCancelEdit}
                className="w-fit rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100"
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      </section>

      <section>
        <h2 className="mb-4 text-lg font-semibold text-gray-900">All Events</h2>

        {isLoading && <p className="text-sm text-gray-600">Loading events...</p>}

        {!isLoading && errorList && (
          <ul className="list-inside list-disc text-sm text-red-600">
            {errorList.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        )}

        {!isLoading && !errorList && errorMessage && <p className="text-sm text-red-600">{errorMessage}</p>}

        {!isLoading && !errorMessage && !errorList && events.length === 0 && (
          <p className="text-sm text-gray-600">No events yet.</p>
        )}

        {!isLoading && !errorMessage && !errorList && events.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-300">
                  <th className="px-3 py-2 text-sm font-medium text-gray-700">Title</th>
                  <th className="px-3 py-2 text-sm font-medium text-gray-700">Status</th>
                  <th className="px-3 py-2 text-sm font-medium text-gray-700">Start Date</th>
                  <th className="px-3 py-2 text-sm font-medium text-gray-700">Seats</th>
                  <th className="px-3 py-2 text-sm font-medium text-gray-700">Actions</th>
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
        )}
      </section>
    </div>
  );
}
