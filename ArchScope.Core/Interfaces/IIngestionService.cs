using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IIngestionService
{
    Task<List<RepoFile>> IngestAsync(string source, CancellationToken ct = default);
}
