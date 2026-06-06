import { AnalysisForm } from '../components/analysis/AnalysisForm';

export function AnalyzePage() {
  return (
    <div className="px-8 py-10">
      <div className="mb-8">
        <h1 className="font-mono text-base uppercase tracking-widest text-arch-muted mb-1">
          Analyze Repository
        </h1>
        <p className="text-sm text-arch-muted/70 font-mono">
          Submit a repository for architectural analysis. Results are saved and browsable in History.
        </p>
      </div>
      <AnalysisForm />
    </div>
  );
}
