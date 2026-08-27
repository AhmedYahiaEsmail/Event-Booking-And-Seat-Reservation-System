// import { Link } from 'react-router-dom';
// import Button from '../components/ui/Button';

// export default function NotFoundPage() {
//   return (
//     <div className="mx-auto flex min-h-[calc(100vh-73px)] max-w-md flex-col items-center justify-center gap-3 px-4 text-center">
//       <p className="font-mono text-sm uppercase tracking-widest text-ink-faint">Error 404</p>
//       <h1 className="font-display text-3xl font-semibold text-ink">This page doesn't exist</h1>
//       <p className="text-sm text-ink-soft">
//         The page you're looking for might have been moved, cancelled, or never existed.
//       </p>
//       <Link to="/" className="mt-2">
//         <Button variant="primary">Back to events</Button>
//       </Link>
//     </div>
//   );
// }


import { Link, useNavigate } from 'react-router-dom';
import { FileQuestion, ArrowLeft, Home } from 'lucide-react';
import Button from '../components/ui/Button';

export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <div className="mx-auto flex min-h-[calc(100vh-73px)] max-w-md flex-col items-center justify-center px-4 py-12 text-center">
      <div className="flex w-full flex-col items-center rounded-2xl border border-line bg-paper p-8 shadow-sm">
        
        {/* Icon Badge */}
        <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10 text-primary">
          <FileQuestion className="h-7 w-7" strokeWidth={2} />
        </div>

        <p className="font-mono text-xs font-semibold uppercase tracking-widest text-ink-faint">
          Error 404
        </p>

        <h1 className="mt-1 font-display text-2xl font-bold tracking-tight text-ink sm:text-3xl">
          Page not found
        </h1>

        <p className="mt-2 text-sm leading-relaxed text-ink-soft">
          The page you're looking for might have been moved, cancelled, or never existed.
        </p>

        {/* Action Buttons */}
        <div className="mt-6 flex flex-wrap items-center justify-center gap-2.5">
          <Button 
            variant="secondary" 
            onClick={() => navigate(-1)}
            className="flex items-center gap-1.5 px-3.5 py-2 text-xs font-medium"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            Go back
          </Button>

          <Link to="/">
            <Button 
              variant="primary" 
              className="flex items-center gap-1.5 px-3.5 py-2 text-xs font-medium"
            >
              <Home className="h-3.5 w-3.5" />
              Back to events
            </Button>
          </Link>
        </div>

      </div>
    </div>
  );
}