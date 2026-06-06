namespace ArchScope.Core.Models;

public class AnalysisJob
{
    public string JobId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public AnalysisJobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ReportJson { get; set; }
}

public enum AnalysisJobStatus
{
    Pending,
    Ingesting,
    Analyzing,
    Completed,
    Failed
}
