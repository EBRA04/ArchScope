using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IAnalysisOrchestrator
{
    Task<AnalysisReport> AnalyzeAsync(
        FileTree fileTree,
        string projectName,
        string sourceType,
        string jobId,
        CancellationToken ct = default);
}
