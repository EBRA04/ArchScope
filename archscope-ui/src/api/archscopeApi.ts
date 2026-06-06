import axios from 'axios';
import type { AnalysisJobResponse, JobSummary } from '../types/archscope';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  timeout: 600_000, // 10 min — analysis takes time
});

export const archscopeApi = {
  analyzeZip: async (file: File, projectName?: string): Promise<AnalysisJobResponse> => {
    const form = new FormData();
    form.append('file', file);
    const params = projectName ? `?projectName=${encodeURIComponent(projectName)}` : '';
    const { data } = await api.post<AnalysisJobResponse>(`/api/analysis/zip${params}`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  },

  analyzeLocal: async (folderPath: string, projectName?: string): Promise<AnalysisJobResponse> => {
    const { data } = await api.post<AnalysisJobResponse>('/api/analysis/local', {
      folderPath,
      projectName: projectName || null,
    });
    return data;
  },

  analyzeGitHub: async (url: string, projectName?: string): Promise<AnalysisJobResponse> => {
    const { data } = await api.post<AnalysisJobResponse>('/api/analysis/github', {
      url,
      projectName: projectName || null,
    });
    return data;
  },

  getJob: async (jobId: string): Promise<AnalysisJobResponse> => {
    const { data } = await api.get<AnalysisJobResponse>(`/api/analysis/${jobId}`);
    return data;
  },

  getMarkdown: async (jobId: string): Promise<string> => {
    const { data } = await api.get<string>(`/api/analysis/${jobId}/markdown`);
    return data;
  },

  listJobs: async (count = 20): Promise<JobSummary[]> => {
    const { data } = await api.get<JobSummary[]>(`/api/jobs?count=${count}`);
    return data;
  },

  deleteJob: async (jobId: string): Promise<void> => {
    await api.delete(`/api/jobs/${jobId}`);
  },
};
