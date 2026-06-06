import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { AlertTriangle, ArrowLeft } from 'lucide-react';
import { archscopeApi } from '../api/archscopeApi';
import type { AnalysisJobResponse } from '../types/archscope';
import { ReportViewer } from '../components/analysis/ReportViewer';
import { StatusBadge } from '../components/shared/StatusBadge';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';

export function ReportPage() {
  const { jobId } = useParams<{ jobId: string }>();
  const navigate = useNavigate();
  const [job, setJob] = useState<AnalysisJobResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  useEffect(() => {
    if (!jobId) return;

    setIsLoading(true);
    setFetchError(null);

    archscopeApi
      .getJob(jobId)
      .then(setJob)
      .catch((err: unknown) => {
        setFetchError(err instanceof Error ? err.message : 'Failed to load report.');
      })
      .finally(() => setIsLoading(false));
  }, [jobId]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner label="Loading report..." />
      </div>
    );
  }

  if (fetchError || !job) {
    return (
      <div className="px-8 py-10">
        <div className="flex items-start gap-3 text-arch-danger border border-arch-danger/30 p-4 max-w-lg">
          <AlertTriangle size={16} className="mt-0.5 flex-shrink-0" />
          <div>
            <p className="font-mono text-xs uppercase tracking-wider mb-1">Error</p>
            <p className="text-sm text-arch-muted">{fetchError ?? 'Report not found.'}</p>
          </div>
        </div>
        <button
          onClick={() => navigate('/history')}
          className="mt-4 arch-btn-ghost flex items-center gap-2"
        >
          <ArrowLeft size={13} /> Back to History
        </button>
      </div>
    );
  }

  if (job.status === 'Failed') {
    return (
      <div className="px-8 py-10">
        <div className="mb-4 flex items-center gap-3">
          <button onClick={() => navigate('/history')} className="arch-btn-ghost flex items-center gap-2">
            <ArrowLeft size={13} /> History
          </button>
          <StatusBadge status={job.status} />
        </div>
        <div className="flex items-start gap-3 text-arch-danger border border-arch-danger/30 p-4 max-w-lg">
          <AlertTriangle size={16} className="mt-0.5 flex-shrink-0" />
          <div>
            <p className="font-mono text-xs uppercase tracking-wider mb-1">Analysis Failed</p>
            <p className="text-sm text-arch-muted">{job.errorMessage ?? 'An unknown error occurred.'}</p>
          </div>
        </div>
      </div>
    );
  }

  if (!job.report) {
    // Job exists but report isn't ready (shouldn't happen with synchronous analysis)
    return (
      <div className="px-8 py-10">
        <div className="flex items-center gap-3 mb-4">
          <StatusBadge status={job.status} />
          <span className="font-mono text-sm text-arch-muted">{job.projectName}</span>
        </div>
        <LoadingSpinner label="Report not yet available..." />
      </div>
    );
  }

  return <ReportViewer report={job.report} />;
}
