using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IJobRepository
{
    Task SaveJobAsync(AnalysisJob job, CancellationToken ct = default);
    Task UpdateJobAsync(AnalysisJob job, CancellationToken ct = default);
    Task<AnalysisJob?> GetJobAsync(string jobId, CancellationToken ct = default);
    Task<List<AnalysisJob>> GetRecentJobsAsync(int count, CancellationToken ct = default);
    Task DeleteJobAsync(string jobId, CancellationToken ct = default);
}
