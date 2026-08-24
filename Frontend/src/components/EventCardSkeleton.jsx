export default function EventCardSkeleton() {
  return (
    <div className="flex flex-col overflow-hidden rounded-2xl border border-line bg-paper shadow-sm">
      <div className="flex flex-col gap-3 p-5">
        <div className="h-3 w-24 animate-pulse rounded bg-surface" />
        <div className="h-5 w-3/4 animate-pulse rounded bg-surface" />
        <div className="h-3 w-1/2 animate-pulse rounded bg-surface" />
        <div className="h-3 w-1/3 animate-pulse rounded bg-surface" />
      </div>
      <div className="border-t border-dashed border-line p-5">
        <div className="h-1.5 w-full animate-pulse rounded-full bg-surface" />
      </div>
    </div>
  );
}