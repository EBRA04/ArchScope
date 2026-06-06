import type { AnalysisReport } from '../../types/archscope';
import { MetadataBadges } from './MetadataBadges';
import { PassResultCard } from './PassResultCard';
import { formatDuration } from '../../types/archscope';
import { Download } from 'lucide-react';

interface ReportViewerProps {
  report: AnalysisReport;
}

const NAV_SECTIONS = [
  { id: 'executive-summary', label: 'Executive Summary' },
  { id: 'structure-analysis', label: 'Structure Analysis' },
  { id: 'module-analysis', label: 'Module Analysis' },
  { id: 'dependency-analysis', label: 'Dependency Analysis' },
  { id: 'dead-code', label: 'Dead Code' },
  { id: 'code-quality', label: 'Code Quality' },
];

export function ReportViewer({ report }: ReportViewerProps) {
  const handleDownload = () => {
    const blob = new Blob([report.fullMarkdownReport], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `archscope-${report.projectName.toLowerCase().replace(/\s+/g, '-')}.md`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="flex gap-0 min-h-screen">
      {/* Left nav — sticky */}
      <aside className="w-52 flex-shrink-0 border-r border-arch-border">
        <div className="sticky top-0 pt-6 pb-6 px-4">
          <div className="mb-4">
            <p className="font-mono text-xs text-arch-muted uppercase tracking-wider mb-1">Project</p>
            <p className="font-mono text-sm text-arch-text font-semibold truncate">{report.projectName}</p>
          </div>

          <div className="mb-4">
            <span className="font-mono text-xs border border-arch-border px-2 py-0.5 text-arch-muted uppercase">
              {report.sourceType}
            </span>
          </div>

          <div className="mb-6">
            <MetadataBadges metadata={report.metadata} />
          </div>

          <div className="mb-6">
            <p className="arch-label mb-2">Duration</p>
            <p className="font-mono text-sm text-arch-text">{formatDuration(report.totalDuration)}</p>
          </div>

          <nav>
            <p className="arch-label mb-2">Jump to</p>
            <ul className="space-y-1">
              {NAV_SECTIONS.map(s => (
                <li key={s.id}>
                  <a
                    href={`#${s.id}`}
                    className="block font-mono text-xs text-arch-muted hover:text-arch-accent transition-colors py-0.5"
                  >
                    › {s.label}
                  </a>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </aside>

      {/* Right content */}
      <main className="flex-1 min-w-0 px-8 py-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="font-mono text-lg font-semibold text-arch-text">{report.projectName}</h1>
            <p className="text-xs text-arch-muted mt-1">
              Generated {new Date(report.generatedAt).toLocaleString()} · {report.metadata.totalFiles} files
            </p>
          </div>
          <button onClick={handleDownload} className="arch-btn-ghost flex items-center gap-2">
            <Download size={14} />
            Download Markdown
          </button>
        </div>

        <div className="space-y-px">
          <PassResultCard
            id="executive-summary"
            result={report.executiveSummary}
            defaultExpanded={true}
          />
          <PassResultCard id="structure-analysis" result={report.structureAnalysis} />
          <PassResultCard id="module-analysis" result={report.moduleAnalysis} />
          <PassResultCard id="dependency-analysis" result={report.dependencyAnalysis} />
          <PassResultCard id="dead-code" result={report.deadCodeAnalysis} />
          <PassResultCard id="code-quality" result={report.qualityAnalysis} />
        </div>
      </main>
    </div>
  );
}
