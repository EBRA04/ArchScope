interface LoadingSpinnerProps {
  label?: string;
  size?: 'sm' | 'md';
}

export function LoadingSpinner({ label, size = 'md' }: LoadingSpinnerProps) {
  return (
    <span
      className={`inline-flex items-center gap-2 font-mono ${
        size === 'sm' ? 'text-xs' : 'text-sm'
      } text-arch-muted`}
    >
      <span className="animate-spin inline-block text-arch-accent">◌</span>
      {label && <span>{label}</span>}
    </span>
  );
}
