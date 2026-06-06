import { NavLink } from 'react-router-dom';
import { ScanLine, History, PlusSquare } from 'lucide-react';

export function Sidebar() {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-2.5 px-4 py-2.5 font-mono text-xs uppercase tracking-wider transition-colors ${
      isActive
        ? 'text-arch-accent border-l-2 border-arch-accent bg-arch-accent/5'
        : 'text-arch-muted border-l-2 border-transparent hover:text-arch-text hover:border-arch-border'
    }`;

  return (
    <aside className="w-48 flex-shrink-0 bg-arch-surface border-r border-arch-border flex flex-col min-h-screen">
      {/* Logo */}
      <div className="px-4 py-5 border-b border-arch-border">
        <div className="flex items-center gap-2">
          <ScanLine size={16} className="text-arch-accent" />
          <span className="font-mono text-sm font-semibold text-arch-text tracking-wider">ARCHSCOPE</span>
        </div>
        <p className="font-mono text-xs text-arch-muted mt-1 leading-tight">repo · analysis</p>
      </div>

      {/* Nav */}
      <nav className="flex-1 py-4">
        <NavLink to="/analyze" className={linkClass}>
          <PlusSquare size={13} />
          Analyze
        </NavLink>
        <NavLink to="/history" className={linkClass}>
          <History size={13} />
          History
        </NavLink>
      </nav>

      {/* Footer */}
      <div className="px-4 py-4 border-t border-arch-border">
        <p className="font-mono text-xs text-arch-muted/50">v0.1.0</p>
      </div>
    </aside>
  );
}
