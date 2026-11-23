export function ErrorAlert({ message, list }) {
  if (!message && !list) return null;

  return (
    <div className="rounded-lg border border-danger/20 bg-danger-soft px-4 py-3">
      {list ? (
        <ul className="list-inside list-disc space-y-1 text-sm text-danger">
          {list.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      ) : (
        <p className="text-sm text-danger">{message}</p>
      )}
    </div>
  );
}

export function SuccessAlert({ message }) {
  if (!message) return null;

  return (
    <div className="rounded-lg border border-success/20 bg-success-soft px-4 py-3">
      <p className="text-sm text-success">{message}</p>
    </div>
  );
}