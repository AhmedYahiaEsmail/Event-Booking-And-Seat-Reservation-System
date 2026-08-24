import { useEffect, useState } from 'react';
import * as eventsApi from '../api/eventsApi';
import { ApiError } from '../api/client';
import EventCard from '../components/EventCard';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { ErrorAlert } from '../components/ui/Alert';
import EmptyState from '../components/ui/EmptyState';
import EventCardSkeleton from '../components/EventCardSkeleton';

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
    <div className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8 flex flex-col gap-1">
        <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Upcoming</p>
        <h1 className="font-display text-2xl font-semibold text-ink">Event Catalog</h1>
      </div>

      <form onSubmit={handleSearchSubmit} className="mb-8 flex gap-2">
        <div className="w-full max-w-sm">
          <Input
            type="text"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder="Search by title, speaker, location..."
          />
        </div>
        <Button type="submit" variant="secondary">
          Search
        </Button>
      </form>

      {isLoading && (
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <EventCardSkeleton key={index} />
          ))}
        </div>
      )}

      <ErrorAlert message={!errorList ? errorMessage : null} list={errorList} />

      {!isLoading && !errorMessage && !errorList && events.length === 0 && (
        <EmptyState
          title="No events found"
          description="Try a different search term, or check back later for new workshops and talks."
        />
      )}

      {!isLoading && !errorMessage && !errorList && events.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {events.map((event) => (
              <EventCard key={event.id} event={event} />
            ))}
          </div>

          <div className="mt-8 flex items-center justify-center gap-3">
            <Button variant="secondary" onClick={handlePrevious} disabled={isPreviousDisabled}>
              Previous
            </Button>
            <span className="font-mono text-xs text-ink-soft">Page {pageNumber}</span>
            <Button variant="secondary" onClick={handleNext} disabled={isNextDisabled}>
              Next
            </Button>
          </div>
        </>
      )}
    </div>
  );
}