// import { useEffect, useState } from 'react';
// import * as eventsApi from '../api/eventsApi';
// import { ApiError } from '../api/client';
// import EventCard from '../components/EventCard';
// import Input from '../components/ui/Input';
// import Button from '../components/ui/Button';
// import { ErrorAlert } from '../components/ui/Alert';
// import EmptyState from '../components/ui/EmptyState';
// import EventCardSkeleton from '../components/EventCardSkeleton';

// const PAGE_SIZE = 12;

// export default function HomePage() {
//   const [events, setEvents] = useState([]);
//   const [isLoading, setIsLoading] = useState(true);
//   const [errorMessage, setErrorMessage] = useState(null);
//   const [errorList, setErrorList] = useState(null);

//   const [searchInput, setSearchInput] = useState('');
//   const [submittedSearchTerm, setSubmittedSearchTerm] = useState('');
//   const [pageNumber, setPageNumber] = useState(1);

//   useEffect(() => {
//     let isCancelled = false;

//     async function fetchEvents() {
//       setIsLoading(true);
//       setErrorMessage(null);
//       setErrorList(null);

//       const trimmedSearchTerm = submittedSearchTerm.trim();

//       try {
//         const data = await eventsApi.getEvents({
//           status: 'Published',
//           searchTerm: trimmedSearchTerm || undefined,
//           pageNumber,
//           pageSize: PAGE_SIZE,
//         });

//         if (!isCancelled) {
//           setEvents(data ?? []);
//         }
//       } catch (error) {
//         if (isCancelled) return;

//         setEvents([]);

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
//   }, [pageNumber, submittedSearchTerm]);

//   function handleSearchSubmit(event) {
//     event.preventDefault();
//     setSubmittedSearchTerm(searchInput.trim());
//     setPageNumber(1);
//   }

//   function handlePrevious() {
//     setPageNumber((current) => Math.max(1, current - 1));
//   }

//   function handleNext() {
//     setPageNumber((current) => current + 1);
//   }

//   const isPreviousDisabled = pageNumber === 1;
//   const isNextDisabled = events.length < PAGE_SIZE;

//   return (
//     <div className="mx-auto max-w-5xl px-4 py-10">
//       <div className="mb-8 flex flex-col gap-1">
//         <p className="font-mono text-xs uppercase tracking-wide text-ink-faint">Upcoming</p>
//         <h1 className="font-display text-2xl font-semibold text-ink">Event Catalog</h1>
//       </div>

//       <form onSubmit={handleSearchSubmit} className="mb-8 flex gap-2">
//         <div className="w-full max-w-sm">
//           <Input
//             type="text"
//             value={searchInput}
//             onChange={(event) => setSearchInput(event.target.value)}
//             placeholder="Search by title, speaker, location..."
//           />
//         </div>
//         <Button type="submit" variant="secondary">
//           Search
//         </Button>
//       </form>

//       {isLoading && (
//         <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
//           {Array.from({ length: 6 }).map((_, index) => (
//             <EventCardSkeleton key={index} />
//           ))}
//         </div>
//       )}

//       <ErrorAlert message={!errorList ? errorMessage : null} list={errorList} />

//       {!isLoading && !errorMessage && !errorList && events.length === 0 && (
//         <EmptyState
//           title="No events found"
//           description="Try a different search term, or check back later for new workshops and talks."
//         />
//       )}

//       {!isLoading && !errorMessage && !errorList && events.length > 0 && (
//         <>
//           <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
//             {events.map((event) => (
//               <EventCard key={event.id} event={event} />
//             ))}
//           </div>

//           <div className="mt-8 flex items-center justify-center gap-3">
//             <Button variant="secondary" onClick={handlePrevious} disabled={isPreviousDisabled}>
//               Previous
//             </Button>
//             <span className="font-mono text-xs text-ink-soft">Page {pageNumber}</span>
//             <Button variant="secondary" onClick={handleNext} disabled={isNextDisabled}>
//               Next
//             </Button>
//           </div>
//         </>
//       )}
//     </div>
//   );
// }

import { useEffect, useState } from 'react';
import { ChevronLeft, ChevronRight, Search, Sparkles, Zap } from 'lucide-react';
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
      {/* Hero Section */}
      <section className="relative mb-10 overflow-hidden rounded-3xl border border-line bg-surface/40 px-6 py-12 sm:px-10 sm:py-16">
        <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-transparent to-primary/10" />
        <div className="relative z-10 flex flex-col items-center text-center">
          <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-line bg-paper/80 px-4 py-1.5 shadow-sm backdrop-blur">
            <Zap className="h-3.5 w-3.5 text-primary" />
            <span className="text-xs font-semibold uppercase tracking-wide text-ink">
              Live Interactive Sessions
            </span>
          </div>

          <h1 className="max-w-2xl font-display text-3xl font-bold leading-tight text-ink sm:text-4xl lg:text-5xl">
            Discover & Book{' '}
            <span className="bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
              Top Tech Events
            </span>
          </h1>

          <p className="mt-4 max-w-lg text-base text-ink-soft sm:text-lg">
            Explore upcoming workshops, masterclasses, and tech talks from industry leaders.
          </p>

          {/* Modernized Search Bar */}
          <form
            onSubmit={handleSearchSubmit}
            className="mt-8 flex w-full max-w-xl items-center gap-2 rounded-2xl border border-line bg-paper/90 p-2 shadow-lg shadow-ink/5 backdrop-blur transition focus-within:border-primary/50 focus-within:ring-2 focus-within:ring-primary/20"
          >
            <div className="flex flex-1 items-center gap-2.5 pl-3">
              <Search className="h-5 w-5 text-ink-soft" />
              <Input
                type="text"
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                placeholder="Search by title, speaker, location..."
                className="border-0 bg-transparent px-0 shadow-none focus-visible:ring-0"
              />
            </div>
            <Button type="submit" variant="primary" className="rounded-xl px-5">
              Search
            </Button>
          </form>
        </div>
      </section>

      {/* Section Divider & Title */}
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="h-4 w-4 text-primary" />
          <h2 className="font-display text-xl font-semibold text-ink sm:text-2xl">
            Featured Catalog
          </h2>
        </div>
        {!isLoading && !errorMessage && !errorList && events.length > 0 && (
          <span className="text-xs text-ink-soft sm:text-sm">
            Showing {events.length} events
          </span>
        )}
      </div>

      {isLoading && (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <EventCardSkeleton key={index} />
          ))}
        </div>
      )}

      <ErrorAlert list={errorList} message={!errorList ? errorMessage : null} />

      {!isLoading && !errorMessage && !errorList && events.length === 0 && (
        <EmptyState
          title="No events found"
          description="Try a different search term, or check back later for new workshops and talks."
        />
      )}

      {!isLoading && !errorMessage && !errorList && events.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {events.map((event) => (
              <EventCard event={event} key={event.id} />
            ))}
          </div>

          {/* Upgraded Pagination */}
          <div className="mt-10 flex items-center justify-center gap-3">
            <Button
              disabled={isPreviousDisabled}
              onClick={handlePrevious}
              variant="secondary"
              className="gap-1 pl-2.5"
            >
              <ChevronLeft className="h-4 w-4" />
              Previous
            </Button>

            <span className="rounded-full border border-line bg-surface/60 px-4 py-1.5 font-mono text-xs font-medium text-ink">
              Page {pageNumber}
            </span>

            <Button
              disabled={isNextDisabled}
              onClick={handleNext}
              variant="secondary"
              className="gap-1 pr-2.5"
            >
              Next
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
