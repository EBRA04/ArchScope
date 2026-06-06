using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ArchScope.Infrastructure.Persistence;

public class JobRepository : IJobRepository
{
    private readonly ArchScopeDbContext _db;

    public JobRepository(ArchScopeDbContext db)
    {
        _db = db;
    }

    public async Task SaveJobAsync(AnalysisJob job, CancellationToken ct = default)
    {
        var entity = ToEntity(job);
        _db.AnalysisJobs.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateJobAsync(AnalysisJob job, CancellationToken ct = default)
    {
        var entity = await _db.AnalysisJobs.FindAsync(new object[] { job.JobId }, ct);
        if (entity == null) return;

        entity.Status = job.Status.ToString();
        entity.CompletedAt = job.CompletedAt;
        entity.ErrorMessage = job.ErrorMessage;
        entity.ReportJson = job.ReportJson;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<AnalysisJob?> GetJobAsync(string jobId, CancellationToken ct = default)
    {
        var entity = await _db.AnalysisJobs.FindAsync(new object[] { jobId }, ct);
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<List<AnalysisJob>> GetRecentJobsAsync(int count, CancellationToken ct = default)
    {
        var entities = await _db.AnalysisJobs
            .OrderByDescending(j => j.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task DeleteJobAsync(string jobId, CancellationToken ct = default)
    {
        var entity = await _db.AnalysisJobs.FindAsync(new object[] { jobId }, ct);
        if (entity != null)
        {
            _db.AnalysisJobs.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static AnalysisJobEntity ToEntity(AnalysisJob job) => new()
    {
        JobId = job.JobId,
        ProjectName = job.ProjectName,
        SourceType = job.SourceType,
        SourcePath = job.SourcePath,
        Status = job.Status.ToString(),
        CreatedAt = job.CreatedAt,
        CompletedAt = job.CompletedAt,
        ErrorMessage = job.ErrorMessage,
        ReportJson = job.ReportJson
    };

    private static AnalysisJob ToDomain(AnalysisJobEntity entity) => new()
    {
        JobId = entity.JobId,
        ProjectName = entity.ProjectName,
        SourceType = entity.SourceType,
        SourcePath = entity.SourcePath,
        Status = Enum.Parse<AnalysisJobStatus>(entity.Status),
        CreatedAt = entity.CreatedAt,
        CompletedAt = entity.CompletedAt,
        ErrorMessage = entity.ErrorMessage,
        ReportJson = entity.ReportJson
    };
}
