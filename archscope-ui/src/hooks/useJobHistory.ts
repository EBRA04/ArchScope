import { useState, useEffect, useCallback } from 'react';
import { archscopeApi } from '../api/archscopeApi';
import type { JobSummary } from '../types/archscope';

interface UseJobHistoryReturn {
  jobs: JobSummary[];
  isLoading: boolean;
  error: string | null;
  refresh: () => void;
  deleteJob: (jobId: string) => Promise<void>;
}

export function useJobHistory(count = 50): UseJobHistoryReturn {
  const [jobs, setJobs] = useState<JobSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await archscopeApi.listJobs(count);
      setJobs(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load jobs.');
    } finally {
      setIsLoading(false);
    }
  }, [count]);

  useEffect(() => {
    load();
  }, [load]);

  const deleteJob = useCallback(
    async (jobId: string) => {
      await archscopeApi.deleteJob(jobId);
      setJobs(prev => prev.filter(j => j.jobId !== jobId));
    },
    []
  );

  return { jobs, isLoading, error, refresh: load, deleteJob };
}
