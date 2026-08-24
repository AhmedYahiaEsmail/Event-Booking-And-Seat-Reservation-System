export default function Input({ label, id, className = '', ...props }) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-ink">
          {label}
        </label>
      )}
      <input
        id={id}
        className={`rounded-lg border border-line bg-paper px-3.5 py-2.5 text-sm text-ink placeholder:text-ink-faint transition focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary-soft ${className}`}
        {...props}
      />
    </div>
  );
}