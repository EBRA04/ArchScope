export interface RepoMetadata {
  totalFiles: number;
  totalDirectories: number;
  totalSizeBytes: number;
  detectedLanguages: string[];
  configFiles: string[];
  detectedFrameworks: string[];
  filesByExtension: Record<string, number>;
  hasTests: boolean;
  hasDockerfile: boolean;
  hasCiCd: boolean;
}

export interface PassResult {
  passName: string;
  content: string;
  success: boolean;
  errorMessage?: string;
  duration: string; // TimeSpan serializes as "00:00:08.2345678"
}

export interface AnalysisReport {
  jobId: string;
  generatedAt: string;
  projectName: string;
  sourceType: string;
  metadata: RepoMetadata;
  structureAnalysis: PassResult;
  moduleAnalysis: PassResult;
  dependencyAnalysis: PassResult;
  deadCodeAnalysis: PassResult;
  qualityAnalysis: PassResult;
  executiveSummary: PassResult;
  fullMarkdownReport: string;
  totalDuration: string;
  isComplete: boolean;
}

export type JobStatus = 'Pending' | 'Ingesting' | 'Analyzing' | 'Completed' | 'Failed';

export interface AnalysisJobResponse {
  jobId: string;
  projectName: string;
  status: JobStatus;
  sourceType: string;
  createdAt: string;
  completedAt?: string;
  errorMessage?: string;
  report?: AnalysisReport;
}

export interface JobSummary {
  jobId: string;
  projectName: string;
  sourceType: string;
  status: string;
  createdAt: string;
  completedAt?: string;
}

// Helper: parse TimeSpan duration string to seconds
export function parseDuration(duration: string): number {
  if (!duration) return 0;
  const parts = duration.split(':');
  if (parts.length < 3) return 0;
  const hours = parseFloat(parts[0]);
  const minutes = parseFloat(parts[1]);
  const seconds = parseFloat(parts[2]);
  return hours * 3600 + minutes * 60 + seconds;
}

export function formatDuration(duration: string): string {
  const secs = parseDuration(duration);
  if (secs < 60) return `${secs.toFixed(1)}s`;
  const m = Math.floor(secs / 60);
  const s = Math.floor(secs % 60);
  return `${m}m ${s}s`;
}
