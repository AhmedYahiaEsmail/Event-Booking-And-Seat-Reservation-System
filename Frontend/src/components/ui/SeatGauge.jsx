export default function SeatGauge({ available, total }) {
  const percentFilled = total > 0 ? Math.round(((total - available) / total) * 100) : 0;

  let barColor = "bg-success";
  let label = "Open";
  if (available === 0) {
    barColor = "bg-danger";
    label = "Sold out";
  } else if (percentFilled >= 80) {
    barColor = "bg-warning";
    label = "Almost full";
  }

  return (
    <div className="flex flex-col gap-1.5">
      <div className="h-1.5 w-full overflow-hidden rounded-full bg-surface">
        <div
          className={`h-full rounded-full ${barColor} transition-[width] duration-500 ease-out`}
          style={{ width: `${percentFilled}%` }}
        />
      </div>
      <div className="flex items-center justify-between">
        <span className="font-mono text-xs tabular-nums text-ink-soft">
          {available}/{total} seats
        </span>
        <span className="font-mono text-[10px] font-medium uppercase tracking-wider text-ink-faint">
          {label}
        </span>
      </div>
    </div>
  );
}