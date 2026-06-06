using ArchScope.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArchScope.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;

    public JobsController(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<JobSummary>>> ListJobs(
        [FromQuery] int count = 20,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);
        var jobs = await _jobRepository.GetRecentJobsAsync(count, ct);
        return jobs.Select(j => new JobSummary
        {
            JobId = j.JobId,
            ProjectName = j.ProjectName,
            SourceType = j.SourceType,
            Status = j.Status.ToString(),
            CreatedAt = j.CreatedAt,
            CompletedAt = j.CompletedAt
        }).ToList();
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(string jobId, CancellationToken ct)
    {
        var job = await _jobRepository.GetJobAsync(jobId, ct);
        if (job == null) return NotFound(new { error = $"Job {jobId} not found." });

        await _jobRepository.DeleteJobAsync(jobId, ct);
        return NoContent();
    }
}

public record JobSummary
{
    public string JobId { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
