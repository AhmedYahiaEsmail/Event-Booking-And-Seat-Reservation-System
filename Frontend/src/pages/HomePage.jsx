import { useEffect } from 'react';
import * as eventsApi from '../api/eventsApi';

export default function HomePage() {
  // TODO: temporary — proves the Vite dev server -> CORS -> API client ->
  // backend chain works end-to-end. Remove once real catalog rendering
  // (fetching + displaying events) is built in a later step.
  useEffect(() => {
    eventsApi
      .getEvents()
      .then((data) => console.log('Events fetched successfully:', data))
      .catch((error) => console.log('Events fetch failed:', error));
  }, []);

  return <h1>Event Catalog</h1>;
}
