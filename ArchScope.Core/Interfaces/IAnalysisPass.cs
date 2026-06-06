using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IAnalysisPass
{
    string PassName { get; }
    Task<PassResult> RunAsync(ChunkContext context, CancellationToken ct = default);
}
