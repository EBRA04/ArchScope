namespace ArchScope.Core.Models;

public class AnalysisReport
{
    public string JobId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public RepoMetadata Metadata { get; set; } = new();
    public PassResult StructureAnalysis { get; set; } = new();
    public PassResult ModuleAnalysis { get; set; } = new();
    public PassResult DependencyAnalysis { get; set; } = new();
    public PassResult DeadCodeAnalysis { get; set; } = new();
    public PassResult QualityAnalysis { get; set; } = new();
    public PassResult ExecutiveSummary { get; set; } = new();
    public string FullMarkdownReport { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public bool IsComplete { get; set; }
}
