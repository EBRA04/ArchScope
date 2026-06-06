import type { RepoMetadata } from '../../types/archscope';

interface MetadataBadgesProps {
  metadata: RepoMetadata;
}

export function MetadataBadges({ metadata }: MetadataBadgesProps) {
  const badges: string[] = [
    ...metadata.detectedLanguages,
    ...metadata.detectedFrameworks,
    `${metadata.totalFiles} files`,
    ...(metadata.hasTests ? ['Tests ✓'] : []),
    ...(metadata.hasDockerfile ? ['Docker ✓'] : []),
    ...(metadata.hasCiCd ? ['CI/CD ✓'] : []),
  ];

  return (
    <div className="flex flex-wrap gap-1.5">
      {badges.map((badge) => (
        <span
          key={badge}
          className="font-mono text-xs text-arch-muted border border-arch-border px-2 py-0.5"
        >
          {badge}
        </span>
      ))}
    </div>
  );
}
