const VARIANTS = {
  primary: "bg-primary text-white hover:bg-primary-hover shadow-sm",
  secondary: "border border-line bg-paper text-ink hover:bg-surface",
  danger: "border border-danger/25 bg-paper text-danger hover:bg-danger-soft",
  ghost: "text-ink-soft hover:text-ink hover:bg-surface",
};

export default function Button({ variant = "primary", className = "", children, ...props }) {
  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition-colors duration-150 disabled:cursor-not-allowed disabled:opacity-50 ${VARIANTS[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}