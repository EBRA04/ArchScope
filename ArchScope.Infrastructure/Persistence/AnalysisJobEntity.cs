namespace ArchScope.Infrastructure.Persistence;

public class AnalysisJobEntity
{
    public string JobId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ReportJson { get; set; }
}
