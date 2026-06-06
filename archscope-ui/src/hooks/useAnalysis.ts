import { useState } from 'react';
import { archscopeApi } from '../api/archscopeApi';

interface UseAnalysisReturn {
  submitZip: (file: File, name?: string) => Promise<string>;
  submitLocal: (path: string, name?: string) => Promise<string>;
  submitGitHub: (url: string, name?: string) => Promise<string>;
  isSubmitting: boolean;
  error: string | null;
}

export function useAnalysis(): UseAnalysisReturn {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const wrap = async (fn: () => Promise<string>): Promise<string> => {
    setIsSubmitting(true);
    setError(null);
    try {
      return await fn();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'An error occurred';
      setError(msg);
      throw err;
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    submitZip: (file, name) =>
      wrap(async () => {
        const result = await archscopeApi.analyzeZip(file, name);
        return result.jobId;
      }),

    submitLocal: (path, name) =>
      wrap(async () => {
        const result = await archscopeApi.analyzeLocal(path, name);
        return result.jobId;
      }),

    submitGitHub: (url, name) =>
      wrap(async () => {
        const result = await archscopeApi.analyzeGitHub(url, name);
        return result.jobId;
      }),

    isSubmitting,
    error,
  };
}
