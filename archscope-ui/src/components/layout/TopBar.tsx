// TopBar is intentionally minimal — the Sidebar handles primary navigation.
// Kept for structural completeness and potential future use.

interface TopBarProps {
  title?: string;
}

export function TopBar({ title }: TopBarProps) {
  if (!title) return null;

  return (
    <div className="border-b border-arch-border px-6 py-3">
      <span className="font-mono text-xs uppercase tracking-widest text-arch-muted">{title}</span>
    </div>
  );
}
