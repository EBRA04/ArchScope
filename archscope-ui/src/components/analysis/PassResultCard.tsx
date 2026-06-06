import { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import { ChevronDown, ChevronRight, AlertTriangle, CheckCircle } from 'lucide-react';
import type { PassResult } from '../../types/archscope';
import { formatDuration } from '../../types/archscope';

interface PassResultCardProps {
  result: PassResult;
  defaultExpanded?: boolean;
  id?: string;
}

export function PassResultCard({ result, defaultExpanded = false, id }: PassResultCardProps) {
  const [expanded, setExpanded] = useState(defaultExpanded);

  return (
    <div id={id} className="border border-arch-border bg-arch-surface">
      {/* Header */}
      <button
        onClick={() => setExpanded(e => !e)}
        className="w-full flex items-center justify-between px-4 py-3 hover:bg-white/[0.02] transition-colors"
      >
        <div className="flex items-center gap-3">
          {expanded
            ? <ChevronDown size={14} className="text-arch-muted" />
            : <ChevronRight size={14} className="text-arch-muted" />
          }
          <span className="font-mono text-xs uppercase tracking-widest text-arch-muted">
            ┌─ {result.passName.toUpperCase()}
          </span>
        </div>
        <div className="flex items-center gap-3">
          <span className="font-mono text-xs text-arch-muted">{formatDuration(result.duration)}</span>
          {result.success
            ? <CheckCircle size={14} className="text-arch-success" />
            : <AlertTriangle size={14} className="text-arch-danger" />
          }
        </div>
      </button>

      {/* Content */}
      {expanded && (
        <div className="border-t border-arch-border px-6 py-5">
          {result.success ? (
            <div className="markdown-content">
              <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                components={{
                  code(codeProps) {
                    const { className, children } = codeProps;
                    const match = /language-(\w+)/.exec(className ?? '');
                    const isBlock = String(children).includes('\n');
                    if (isBlock) {
                      return (
                        <SyntaxHighlighter
                          style={vscDarkPlus}
                          language={match ? match[1] : 'text'}
                          customStyle={{
                            background: '#1e1e2e',
                            fontSize: '0.75rem',
                            borderRadius: 0,
                            margin: '0.75rem 0',
                          }}
                        >
                          {String(children).replace(/\n$/, '')}
                        </SyntaxHighlighter>
                      );
                    }
                    return <code className={className}>{children}</code>;
                  },
                }}
              >
                {result.content}
              </ReactMarkdown>
            </div>
          ) : (
            <div className="flex items-start gap-3 text-arch-danger">
              <AlertTriangle size={16} className="mt-0.5 flex-shrink-0" />
              <div>
                <p className="font-mono text-xs uppercase tracking-wider mb-1">Pass Failed</p>
                <p className="text-sm text-arch-muted">{result.errorMessage}</p>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
