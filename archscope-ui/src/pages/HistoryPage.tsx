import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Trash2, RefreshCw, AlertTriangle } from 'lucide-react';
import { useJobHistory } from '../hooks/useJobHistory';
import { StatusBadge } from '../components/shared/StatusBadge';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';

export function HistoryPage() {
  const navigate = useNavigate();
  const { jobs, isLoading, error, refresh, deleteJob } = useJobHistory(100);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (e: React.MouseEvent, jobId: string) => {
    e.stopPropagation();
    if (!confirm('Delete this analysis? This cannot be undone.')) return;
    setDeletingId(jobId);
    try {
      await deleteJob(jobId);
    } finally {
      setDeletingId(null);
    }
  };

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });

  const getDuration = (job: { createdAt: string; completedAt?: string }) => {
    if (!job.completedAt) return '—';
    const ms = new Date(job.completedAt).getTime() - new Date(job.createdAt).getTime();
    const secs = ms / 1000;
    if (secs < 60) return `${Math.round(secs)}s`;
    return `${Math.floor(secs / 60)}m ${Math.round(secs % 60)}s`;
  };

  return (
    <div className="px-8 py-10">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="font-mono text-base uppercase tracking-widest text-arch-muted mb-1">
            Analysis History
          </h1>
          <p className="text-sm text-arch-muted/70 font-mono">
            {jobs.length > 0 ? `${jobs.length} analyses` : 'No analyses yet'}
          </p>
        </div>
        <button
          onClick={refresh}
          disabled={isLoading}
          className="arch-btn-ghost flex items-center gap-2"
        >
          <RefreshCw size={13} className={isLoading ? 'animate-spin' : ''} />
          Refresh
        </button>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex items-center gap-3 py-8">
          <LoadingSpinner label="Loading analyses..." />
        </div>
      )}

      {/* Error */}
      {error && !isLoading && (
        <div className="flex items-start gap-2 text-arch-danger border border-arch-danger/30 p-3 max-w-lg mb-4">
          <AlertTriangle size={14} className="mt-0.5 flex-shrink-0" />
          <span className="font-mono text-xs">{error}</span>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !error && jobs.length === 0 && (
        <div className="border border-arch-border p-10 text-center max-w-md">
          <p className="font-mono text-sm text-arch-muted">No analyses yet.</p>
          <p className="font-mono text-xs text-arch-muted/60 mt-2">
            Head to{' '}
            <button
              onClick={() => navigate('/analyze')}
              className="text-arch-accent hover:underline"
            >
              Analyze
            </button>{' '}
            to inspect your first repository.
          </p>
        </div>
      )}

      {/* Table */}
      {!isLoading && jobs.length > 0 && (
        <div className="border border-arch-border overflow-hidden">
          {/* Table header */}
          <div className="grid grid-cols-[2fr_80px_110px_160px_80px_48px] bg-arch-surface border-b border-arch-border">
            {['Project', 'Source', 'Status', 'Date', 'Duration', ''].map(col => (
              <div key={col} className="px-4 py-2.5 font-mono text-xs uppercase tracking-wider text-arch-muted">
                {col}
              </div>
            ))}
          </div>

          {/* Rows */}
          {jobs.map((job, i) => (
            <div
              key={job.jobId}
              onClick={() => job.status === 'Completed' && navigate(`/report/${job.jobId}`)}
              className={`grid grid-cols-[2fr_80px_110px_160px_80px_48px] items-center transition-colors ${
                i > 0 ? 'border-t border-arch-border' : ''
              } ${
                job.status === 'Completed'
                  ? 'cursor-pointer hover:bg-white/[0.02]'
                  : 'opacity-75'
              }`}
            >
              {/* Project name */}
              <div className="px-4 py-3 min-w-0">
                <span className="font-mono text-sm text-arch-text truncate block">
                  {job.projectName}
                </span>
              </div>

              {/* Source */}
              <div className="px-4 py-3">
                <span className="font-mono text-xs text-arch-muted border border-arch-border px-1.5 py-0.5">
                  {job.sourceType}
                </span>
              </div>

              {/* Status */}
              <div className="px-4 py-3">
                <StatusBadge status={job.status} />
              </div>

              {/* Date */}
              <div className="px-4 py-3">
                <span className="font-mono text-xs text-arch-muted tabular-nums">
                  {formatDate(job.createdAt)}
                </span>
              </div>

              {/* Duration */}
              <div className="px-4 py-3">
                <span className="font-mono text-xs text-arch-muted tabular-nums">
                  {getDuration(job)}
                </span>
              </div>

              {/* Delete */}
              <div className="px-3 py-3 flex items-center justify-center">
                <button
                  onClick={e => handleDelete(e, job.jobId)}
                  disabled={deletingId === job.jobId}
                  className="p-1.5 text-arch-muted hover:text-arch-danger transition-colors disabled:opacity-40"
                  title="Delete analysis"
                >
                  <Trash2 size={13} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
