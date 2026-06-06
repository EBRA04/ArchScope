import { useEffect, useState, useRef } from 'react';
import { archscopeApi } from '../api/archscopeApi';
import type { AnalysisJobResponse } from '../types/archscope';

const PASS_ORDER = [
  'Structure Analysis',
  'Module Analysis',
  'Dependency Analysis',
  'Dead Code Detection',
  'Code Quality Analysis',
  'Executive Summary',
];

interface UseJobPollingReturn {
  job: AnalysisJobResponse | null;
  isLoading: boolean;
  isComplete: boolean;
  isFailed: boolean;
  currentPass: string;
  completedPasses: string[];
}

export function useJobPolling(jobId: string | null): UseJobPollingReturn {
  const [job, setJob] = useState<AnalysisJobResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const isComplete = job?.status === 'Completed';
  const isFailed = job?.status === 'Failed';

  // Infer current pass from report state
  const completedPasses: string[] = [];
  if (job?.report) {
    const r = job.report;
    if (r.structureAnalysis?.success) completedPasses.push('Structure Analysis');
    if (r.moduleAnalysis?.success) completedPasses.push('Module Analysis');
    if (r.dependencyAnalysis?.success) completedPasses.push('Dependency Analysis');
    if (r.deadCodeAnalysis?.success) completedPasses.push('Dead Code Detection');
    if (r.qualityAnalysis?.success) completedPasses.push('Code Quality Analysis');
    if (r.executiveSummary?.success) completedPasses.push('Executive Summary');
  }

  const currentPass = job?.status === 'Analyzing'
    ? PASS_ORDER.find(p => !completedPasses.includes(p)) ?? 'Analyzing...'
    : job?.status === 'Ingesting'
    ? 'Ingesting repository...'
    : job?.status === 'Pending'
    ? 'Queued...'
    : '';

  useEffect(() => {
    if (!jobId) return;

    setIsLoading(true);

    const poll = async () => {
      try {
        const result = await archscopeApi.getJob(jobId);
        setJob(result);
        if (result.status === 'Completed' || result.status === 'Failed') {
          setIsLoading(false);
          if (intervalRef.current) clearInterval(intervalRef.current);
        }
      } catch {
        setIsLoading(false);
        if (intervalRef.current) clearInterval(intervalRef.current);
      }
    };

    poll();
    intervalRef.current = setInterval(poll, 3000);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [jobId]);

  return { job, isLoading, isComplete, isFailed, currentPass, completedPasses };
}
