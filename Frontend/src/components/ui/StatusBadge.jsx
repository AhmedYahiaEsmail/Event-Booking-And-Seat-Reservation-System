const STATUS_STYLES = {
  Draft: "bg-surface text-ink-soft ring-1 ring-inset ring-line",
  Published: "bg-success-soft text-success",
  Cancelled: "bg-danger-soft text-danger",
  Completed: "bg-primary-soft text-primary",
  Confirmed: "bg-success-soft text-success",
};

export default function StatusBadge({ status, label }) {
  const style = STATUS_STYLES[status] ?? "bg-surface text-ink-soft ring-1 ring-inset ring-line";

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 font-mono text-[11px] font-semibold uppercase tracking-wide ${style}`}
    >
      {label ?? status}
    </span>
  );
}