interface StatusBadgeProps {
  status: string;
}

interface StatusConfig {
  label: string;
  className: string;
  pulse?: boolean;
}

const STATUS_CONFIG: Record<string, StatusConfig> = {
  Pending:   { label: 'PENDING',   className: 'text-arch-muted border-arch-muted' },
  Ingesting: { label: 'INGESTING', className: 'text-arch-warning border-arch-warning' },
  Analyzing: { label: 'ANALYZING', className: 'text-arch-accent border-arch-accent', pulse: true },
  Completed: { label: 'COMPLETED', className: 'text-arch-success border-arch-success' },
  Failed:    { label: 'FAILED',    className: 'text-arch-danger border-arch-danger' },
};

export function StatusBadge({ status }: StatusBadgeProps) {
  const config: StatusConfig = STATUS_CONFIG[status] ?? {
    label: status.toUpperCase(),
    className: 'text-arch-muted border-arch-muted',
  };

  return (
    <span
      className={`inline-flex items-center gap-1.5 font-mono text-xs border px-2 py-0.5 ${config.className}`}
    >
      {config.pulse && (
        <span className="relative flex h-1.5 w-1.5">
          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-arch-accent opacity-75" />
          <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-arch-accent" />
        </span>
      )}
      {config.label}
    </span>
  );
}
