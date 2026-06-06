using System.Text.Json;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using ArchScope.Services.Ingestion;
using Microsoft.AspNetCore.Mvc;

namespace ArchScope.API.Controllers;

[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly ZipIngestionService _zipIngestion;
    private readonly LocalFolderIngestionService _localIngestion;
    private readonly GitHubIngestionService _githubIngestion;
    private readonly IFileTreeAnalyzer _fileTreeAnalyzer;
    private readonly IAnalysisOrchestrator _orchestrator;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        ZipIngestionService zipIngestion,
        LocalFolderIngestionService localIngestion,
        GitHubIngestionService githubIngestion,
        IFileTreeAnalyzer fileTreeAnalyzer,
        IAnalysisOrchestrator orchestrator,
        IJobRepository jobRepository,
        ILogger<AnalysisController> logger)
    {
        _zipIngestion = zipIngestion;
        _localIngestion = localIngestion;
        _githubIngestion = githubIngestion;
        _fileTreeAnalyzer = fileTreeAnalyzer;
        _orchestrator = orchestrator;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    [HttpPost("zip")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
    public async Task<ActionResult<AnalysisJobResponse>> AnalyzeZip(
        IFormFile? file,
        [FromQuery] string? projectName,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .zip files are accepted." });

        var effectiveName = projectName ?? Path.GetFileNameWithoutExtension(file.FileName);
        var tempFile = Path.GetTempFileName() + ".zip";

        try
        {
            await using (var stream = System.IO.File.Create(tempFile))
                await file.CopyToAsync(stream, ct);

            return await RunAnalysis(tempFile, effectiveName, "zip", ct,
                source => _zipIngestion.IngestAsync(source, ct));
        }
        finally
        {
            if (System.IO.File.Exists(tempFile))
                System.IO.File.Delete(tempFile);
        }
    }

    [HttpPost("local")]
    public async Task<ActionResult<AnalysisJobResponse>> AnalyzeLocal(
        [FromBody] LocalAnalysisRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FolderPath))
            return BadRequest(new { error = "FolderPath is required." });

        if (!Directory.Exists(request.FolderPath))
            return BadRequest(new { error = $"Folder not found: {request.FolderPath}" });

        var effectiveName = request.ProjectName ?? new DirectoryInfo(request.FolderPath).Name;
        return await RunAnalysis(request.FolderPath, effectiveName, "local", ct,
            source => _localIngestion.IngestAsync(source, ct));
    }

    [HttpPost("github")]
    public async Task<ActionResult<AnalysisJobResponse>> AnalyzeGitHub(
        [FromBody] GitHubAnalysisRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { error = "URL is required." });

        var uri = new Uri(request.Url);
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        var effectiveName = request.ProjectName ?? (segments.Length >= 2 ? segments[1] : "unknown");

        return await RunAnalysis(request.Url, effectiveName, "github", ct,
            source => _githubIngestion.IngestAsync(source, ct));
    }

    [HttpGet("{jobId}")]
    public async Task<ActionResult<AnalysisJobResponse>> GetJob(string jobId, CancellationToken ct)
    {
        var job = await _jobRepository.GetJobAsync(jobId, ct);
        if (job == null) return NotFound(new { error = $"Job {jobId} not found." });
        return ToResponse(job);
    }

    [HttpGet("{jobId}/markdown")]
    public async Task<IActionResult> GetMarkdown(string jobId, CancellationToken ct)
    {
        var job = await _jobRepository.GetJobAsync(jobId, ct);
        if (job == null) return NotFound(new { error = $"Job {jobId} not found." });

        if (job.ReportJson == null)
            return NotFound(new { error = "Report not yet available." });

        var report = JsonSerializer.Deserialize<AnalysisReport>(job.ReportJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (report == null)
            return StatusCode(500, new { error = "Failed to deserialize report." });

        return Content(report.FullMarkdownReport, "text/markdown");
    }

    private async Task<ActionResult<AnalysisJobResponse>> RunAnalysis(
        string source,
        string projectName,
        string sourceType,
        CancellationToken ct,
        Func<string, Task<List<RepoFile>>> ingestFunc)
    {
        var job = new AnalysisJob
        {
            JobId = Guid.NewGuid().ToString("N"),
            ProjectName = projectName,
            SourceType = sourceType,
            SourcePath = source,
            Status = AnalysisJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _jobRepository.SaveJobAsync(job, ct);

        try
        {
            job.Status = AnalysisJobStatus.Ingesting;
            await _jobRepository.UpdateJobAsync(job, ct);

            var files = await ingestFunc(source);
            var fileTree = _fileTreeAnalyzer.BuildTree(files, projectName);

            job.Status = AnalysisJobStatus.Analyzing;
            await _jobRepository.UpdateJobAsync(job, ct);

            var report = await _orchestrator.AnalyzeAsync(fileTree, projectName, sourceType, job.JobId, ct);

            job.Status = AnalysisJobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.ReportJson = JsonSerializer.Serialize(report);
            await _jobRepository.UpdateJobAsync(job, ct);

            return ToResponse(job, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for job {JobId}", job.JobId);
            job.Status = AnalysisJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateJobAsync(job, ct);
            return StatusCode(500, new { error = ex.Message, jobId = job.JobId });
        }
    }

    private static AnalysisJobResponse ToResponse(AnalysisJob job, AnalysisReport? report = null)
    {
        if (report == null && job.ReportJson != null)
        {
            try
            {
                report = JsonSerializer.Deserialize<AnalysisReport>(job.ReportJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { /* ignore */ }
        }

        return new AnalysisJobResponse
        {
            JobId = job.JobId,
            ProjectName = job.ProjectName,
            Status = job.Status.ToString(),
            SourceType = job.SourceType,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage,
            Report = report
        };
    }
}

public record LocalAnalysisRequest(string FolderPath, string? ProjectName);
public record GitHubAnalysisRequest(string Url, string? ProjectName);

public record AnalysisJobResponse
{
    public string JobId { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public AnalysisReport? Report { get; init; }
}
