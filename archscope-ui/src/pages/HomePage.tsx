import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Upload, FolderOpen, Github, ArrowRight } from 'lucide-react';
import { archscopeApi } from '../api/archscopeApi';
import type { JobSummary } from '../types/archscope';
import { StatusBadge } from '../components/shared/StatusBadge';

export function HomePage() {
  const [jobs, setJobs] = useState<JobSummary[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    archscopeApi.listJobs(5).then(setJobs).catch(() => {});
  }, []);

  const inputCards = [
    {
      icon: <Github size={20} className="text-arch-accent" />,
      title: 'GitHub Repository',
      desc: 'Analyze any public GitHub repo by URL. Branches supported.',
      action: () => navigate('/analyze'),
    },
    {
      icon: <Upload size={20} className="text-arch-accent" />,
      title: 'ZIP Archive',
      desc: 'Upload a .zip of any project. Works with GitHub downloads.',
      action: () => navigate('/analyze'),
    },
    {
      icon: <FolderOpen size={20} className="text-arch-accent" />,
      title: 'Local Folder',
      desc: 'Provide an absolute path to a project on this machine.',
      action: () => navigate('/analyze'),
    },
  ];

  return (
    <div className="px-8 py-12 max-w-4xl">
      {/* Hero */}
      <div className="mb-12">
        <h1 className="font-mono text-4xl font-semibold text-arch-text tracking-tight mb-3">
          ARCHSCOPE
        </h1>
        <p className="font-mono text-arch-muted text-sm max-w-lg leading-relaxed">
          Repository analysis and architecture inspection. Powered by AI. Built for engineers.
        </p>
      </div>

      {/* Input cards */}
      <div className="grid grid-cols-3 gap-px bg-arch-border mb-12">
        {inputCards.map((card) => (
          <div
            key={card.title}
            onClick={card.action}
            className="bg-arch-surface p-6 cursor-pointer hover:bg-white/[0.02] transition-colors group"
          >
            <div className="mb-4">{card.icon}</div>
            <h3 className="font-mono text-sm font-semibold text-arch-text mb-2">{card.title}</h3>
            <p className="text-xs text-arch-muted leading-relaxed mb-4">{card.desc}</p>
            <div className="flex items-center gap-1 font-mono text-xs text-arch-accent opacity-0 group-hover:opacity-100 transition-opacity">
              Analyze <ArrowRight size={12} />
            </div>
          </div>
        ))}
      </div>

      {/* Recent analyses */}
      {jobs.length > 0 && (
        <div>
          <div className="flex items-center justify-between mb-3">
            <p className="arch-label">Recent Analyses</p>
            <Link to="/history" className="font-mono text-xs text-arch-accent hover:underline">
              View all →
            </Link>
          </div>
          <div className="border border-arch-border">
            {jobs.map((job, i) => (
              <Link
                key={job.jobId}
                to={`/report/${job.jobId}`}
                className={`flex items-center justify-between px-4 py-3 hover:bg-white/[0.02] transition-colors ${
                  i > 0 ? 'border-t border-arch-border' : ''
                }`}
              >
                <div className="flex items-center gap-3 min-w-0">
                  <span className="font-mono text-sm text-arch-text truncate">{job.projectName}</span>
                  <span className="font-mono text-xs text-arch-muted border border-arch-border px-1.5 py-0.5 flex-shrink-0">
                    {job.sourceType}
                  </span>
                </div>
                <div className="flex items-center gap-3 flex-shrink-0">
                  <StatusBadge status={job.status} />
                  <span className="font-mono text-xs text-arch-muted">
                    {new Date(job.createdAt).toLocaleDateString()}
                  </span>
                </div>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
