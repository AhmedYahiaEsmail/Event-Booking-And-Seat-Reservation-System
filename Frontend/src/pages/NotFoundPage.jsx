import { Link } from 'react-router-dom';
import Button from '../components/ui/Button';

export default function NotFoundPage() {
  return (
    <div className="mx-auto flex min-h-[calc(100vh-73px)] max-w-md flex-col items-center justify-center gap-3 px-4 text-center">
      <p className="font-mono text-sm uppercase tracking-widest text-ink-faint">Error 404</p>
      <h1 className="font-display text-3xl font-semibold text-ink">This page doesn't exist</h1>
      <p className="text-sm text-ink-soft">
        The page you're looking for might have been moved, cancelled, or never existed.
      </p>
      <Link to="/" className="mt-2">
        <Button variant="primary">Back to events</Button>
      </Link>
    </div>
  );
}