import { useEffect, useState } from 'react';
import * as eventsApi from '../api/eventsApi';
import { ApiError } from '../api/client';
import EventCard from '../components/EventCard';

const PAGE_SIZE = 12;

export default function HomePage() {
  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [errorList, setErrorList] = useState(null);

  const [searchInput, setSearchInput] = useState('');
  const [submittedSearchTerm, setSubmittedSearchTerm] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  useEffect(() => {
    let isCancelled = false;

    async function fetchEvents() {
      setIsLoading(true);
      setErrorMessage(null);
      setErrorList(null);

      const trimmedSearchTerm = submittedSearchTerm.trim();

      try {
        const data = await eventsApi.getEvents({
          status: 'Published',
          // Omit the key entirely rather than sending an empty string.
          searchTerm: trimmedSearchTerm || undefined,
          pageNumber,
          pageSize: PAGE_SIZE,
        });

        if (!isCancelled) {
          setEvents(data ?? []);
        }
      } catch (error) {
        if (isCancelled) return;

        setEvents([]);

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
  }, [pageNumber, submittedSearchTerm]);

  function handleSearchSubmit(event) {
    event.preventDefault();
    setSubmittedSearchTerm(searchInput.trim());
    setPageNumber(1);
  }

  function handlePrevious() {
    setPageNumber((current) => Math.max(1, current - 1));
  }

  function handleNext() {
    setPageNumber((current) => current + 1);
  }

  const isPreviousDisabled = pageNumber === 1;
  const isNextDisabled = events.length < PAGE_SIZE;

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="mb-6 text-xl font-semibold text-gray-900">Event Catalog</h1>

      <form onSubmit={handleSearchSubmit} className="mb-6 flex gap-2">
        <input
          type="text"
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          placeholder="Search events..."
          className="w-full max-w-sm rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-gray-500 focus:outline-none"
        />
        <button
          type="submit"
          className="rounded-md bg-gray-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-gray-700"
        >
          Search
        </button>
      </form>

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
        <p className="text-sm text-gray-600">No events found.</p>
      )}

      {!isLoading && !errorMessage && !errorList && events.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {events.map((event) => (
              <EventCard key={event.id} event={event} />
            ))}
          </div>

          <div className="mt-6 flex items-center justify-center gap-3">
            <button
              type="button"
              onClick={handlePrevious}
              disabled={isPreviousDisabled}
              className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Previous
            </button>
            <span className="text-sm text-gray-600">Page {pageNumber}</span>
            <button
              type="button"
              onClick={handleNext}
              disabled={isNextDisabled}
              className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
