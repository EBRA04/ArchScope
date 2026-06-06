import { useState, useRef, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Upload, FolderOpen, Github, CheckCircle, AlertCircle } from 'lucide-react';
import { useAnalysis } from '../../hooks/useAnalysis';

type Tab = 'github' | 'zip' | 'local';

const PASS_ORDER = [
  'Structure Analysis',
  'Module Analysis',
  'Dependency Analysis',
  'Dead Code Detection',
  'Code Quality Analysis',
  'Executive Summary',
];

// Approximate time each pass takes (seconds). Used for simulated progress only.
const PASS_DURATIONS = [20, 25, 20, 20, 25, 20];

export function AnalysisForm() {
  const [tab, setTab] = useState<Tab>('github');
  const [file, setFile] = useState<File | null>(null);
  const [localPath, setLocalPath] = useState('');
  const [githubUrl, setGithubUrl] = useState('');
  const [projectName, setProjectName] = useState('');

  // Analysis state
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [completedJobId, setCompletedJobId] = useState<string | null>(null);
  const [analysisError, setAnalysisError] = useState<string | null>(null);

  // Simulated progress state
  const [elapsed, setElapsed] = useState(0);
  const [activePassIndex, setActivePassIndex] = useState(0);
  const [simulatedDone, setSimulatedDone] = useState<Set<number>>(new Set());

  const fileInputRef = useRef<HTMLInputElement>(null);
  const startTimeRef = useRef<number>(0);
  const navigate = useNavigate();

  const { submitZip, submitLocal, submitGitHub, isSubmitting, error } = useAnalysis();

  // Navigate when job completes
  useEffect(() => {
    if (completedJobId) {
      navigate(`/report/${completedJobId}`);
    }
  }, [completedJobId, navigate]);

  // Elapsed timer while analyzing
  useEffect(() => {
    if (!isAnalyzing) {
      setElapsed(0);
      return;
    }
    startTimeRef.current = Date.now();
    const timer = setInterval(() => {
      setElapsed(Math.floor((Date.now() - startTimeRef.current) / 1000));
    }, 1000);
    return () => clearInterval(timer);
  }, [isAnalyzing]);

  // Simulated pass animation — advances through passes based on estimated durations
  useEffect(() => {
    if (!isAnalyzing) {
      setActivePassIndex(0);
      setSimulatedDone(new Set());
      return;
    }

    const timers: ReturnType<typeof setTimeout>[] = [];

    let cumulativeMs = 0;
    PASS_DURATIONS.forEach((secs, i) => {
      // Mark pass as "starting"
      const startTimer = setTimeout(() => {
        setActivePassIndex(i);
      }, cumulativeMs * 1000);
      timers.push(startTimer);

      cumulativeMs += secs;

      // Mark pass as "done" a few seconds before the next starts
      const doneTimer = setTimeout(() => {
        setSimulatedDone(prev => new Set(prev).add(i));
        if (i + 1 < PASS_ORDER.length) setActivePassIndex(i + 1);
      }, cumulativeMs * 1000 - 3000);
      timers.push(doneTimer);
    });

    return () => timers.forEach(clearTimeout);
  }, [isAnalyzing]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    const dropped = e.dataTransfer.files[0];
    if (dropped?.name.endsWith('.zip')) setFile(dropped);
  }, []);

  const handleSubmit = async () => {
    if (isAnalyzing || isSubmitting) return;

    setAnalysisError(null);
    setIsAnalyzing(true);

    try {
      const name = projectName.trim() || undefined;
      let jobId: string;

      if (tab === 'zip' && file) {
        jobId = await submitZip(file, name);
      } else if (tab === 'local' && localPath.trim()) {
        jobId = await submitLocal(localPath.trim(), name);
      } else if (tab === 'github' && githubUrl.trim()) {
        jobId = await submitGitHub(githubUrl.trim(), name);
      } else {
        setIsAnalyzing(false);
        return;
      }

      setCompletedJobId(jobId);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Analysis failed.';
      setAnalysisError(msg);
      setIsAnalyzing(false);
    }
  };

  const canSubmit =
    !isAnalyzing &&
    !isSubmitting &&
    ((tab === 'zip' && !!file) ||
      (tab === 'local' && !!localPath.trim()) ||
      (tab === 'github' && !!githubUrl.trim()));

  const displayError = analysisError ?? error;

  const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'github', label: 'GitHub URL',  icon: <Github size={14} /> },
    { id: 'zip',    label: 'ZIP Upload',  icon: <Upload size={14} /> },
    { id: 'local',  label: 'Local Path',  icon: <FolderOpen size={14} /> },
  ];

  return (
    <div className="max-w-2xl w-full">
      {/* Tab bar */}
      <div className="flex border-b border-arch-border">
        {tabs.map(t => (
          <button
            key={t.id}
            onClick={() => !isAnalyzing && setTab(t.id)}
            disabled={isAnalyzing}
            className={`flex items-center gap-2 px-5 py-3 font-mono text-xs uppercase tracking-wider border-b-2 transition-colors ${
              tab === t.id
                ? 'border-arch-accent text-arch-accent'
                : 'border-transparent text-arch-muted hover:text-arch-text'
            } disabled:opacity-50 disabled:cursor-not-allowed`}
          >
            {t.icon}
            {t.label}
          </button>
        ))}
      </div>

      {/* Form panel */}
      <div className="arch-card p-6 space-y-4">
        {/* GitHub URL */}
        {tab === 'github' && (
          <div>
            <label className="arch-label block mb-2">GitHub URL</label>
            <input
              type="text"
              value={githubUrl}
              onChange={e => setGithubUrl(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && canSubmit && handleSubmit()}
              placeholder="https://github.com/owner/repo"
              className="arch-input"
              disabled={isAnalyzing}
            />
          </div>
        )}

        {/* ZIP Upload */}
        {tab === 'zip' && (
          <div>
            <label className="arch-label block mb-2">ZIP File</label>
            <div
              onDrop={handleDrop}
              onDragOver={e => e.preventDefault()}
              onClick={() => !isAnalyzing && fileInputRef.current?.click()}
              className="border border-dashed border-arch-border p-8 text-center cursor-pointer hover:border-arch-accent transition-colors"
            >
              {file ? (
                <div className="font-mono text-sm text-arch-text flex items-center justify-center gap-2">
                  <CheckCircle size={14} className="text-arch-success" />
                  {file.name}
                  <span className="text-arch-muted">
                    ({(file.size / 1024 / 1024).toFixed(1)} MB)
                  </span>
                </div>
              ) : (
                <div className="text-arch-muted text-sm font-mono">
                  <Upload size={24} className="mx-auto mb-2 opacity-40" />
                  Drop .zip file here or click to browse
                </div>
              )}
            </div>
            <input
              ref={fileInputRef}
              type="file"
              accept=".zip"
              className="hidden"
              onChange={e => setFile(e.target.files?.[0] ?? null)}
            />
          </div>
        )}

        {/* Local Path */}
        {tab === 'local' && (
          <div>
            <label className="arch-label block mb-2">Absolute Folder Path</label>
            <input
              type="text"
              value={localPath}
              onChange={e => setLocalPath(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && canSubmit && handleSubmit()}
              placeholder="C:\Users\you\projects\my-app"
              className="arch-input"
              disabled={isAnalyzing}
            />
          </div>
        )}

        {/* Optional project name */}
        <div>
          <label className="arch-label block mb-2">Project Name (optional)</label>
          <input
            type="text"
            value={projectName}
            onChange={e => setProjectName(e.target.value)}
            placeholder="my-project"
            className="arch-input"
            disabled={isAnalyzing}
          />
        </div>

        {/* Error state */}
        {displayError && !isAnalyzing && (
          <div className="flex items-start gap-2 text-arch-danger border border-arch-danger/30 p-3">
            <AlertCircle size={15} className="mt-0.5 flex-shrink-0" />
            <span className="font-mono text-xs leading-relaxed">{displayError}</span>
          </div>
        )}

        {/* Progress panel */}
        {isAnalyzing && (
          <div className="border border-arch-border bg-arch-bg p-4 space-y-3">
            <div className="flex items-center justify-between mb-1">
              <span className="font-mono text-xs text-arch-accent">
                {PASS_ORDER[activePassIndex] ?? 'Finalizing...'}
              </span>
              <span className="font-mono text-xs text-arch-muted tabular-nums">
                {elapsed}s elapsed
              </span>
            </div>
            <div className="space-y-2">
              {PASS_ORDER.map((pass, i) => {
                const done = simulatedDone.has(i);
                const active = !done && i === activePassIndex;
                return (
                  <div key={pass} className="flex items-center gap-2.5">
                    {done ? (
                      <CheckCircle size={12} className="text-arch-success flex-shrink-0" />
                    ) : (
                      <span
                        className={`w-3 h-3 border flex-shrink-0 ${
                          active
                            ? 'border-arch-accent animate-pulse bg-arch-accent/10'
                            : 'border-arch-border'
                        }`}
                      />
                    )}
                    <span
                      className={`font-mono text-xs ${
                        done
                          ? 'text-arch-success'
                          : active
                          ? 'text-arch-accent'
                          : 'text-arch-muted'
                      }`}
                    >
                      {pass}
                    </span>
                  </div>
                );
              })}
            </div>
            <p className="font-mono text-xs text-arch-muted/60 pt-1">
              Analysis can take 2–10 minutes depending on repository size.
            </p>
          </div>
        )}

        {/* Submit button */}
        <button
          onClick={handleSubmit}
          disabled={!canSubmit}
          className={`w-full py-3 font-mono text-sm uppercase tracking-widest border transition-colors ${
            canSubmit
              ? 'bg-arch-accent border-arch-accent text-white hover:bg-blue-600 hover:border-blue-600 cursor-pointer'
              : 'bg-arch-surface border-arch-border text-arch-muted cursor-not-allowed'
          }`}
        >
          {isAnalyzing ? 'ANALYZING...' : 'ANALYZE REPOSITORY'}
        </button>
      </div>
    </div>
  );
}
