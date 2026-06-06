using System.Text;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;

namespace ArchScope.Services.Report;

public class ReportService : IReportService
{
    public string GenerateMarkdown(AnalysisReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# ArchScope Analysis: {report.ProjectName}");
        sb.AppendLine();
        sb.AppendLine($"> Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC | Source: {report.SourceType} | Duration: {report.TotalDuration.TotalSeconds:F1}s");
        sb.AppendLine();

        // Repository Overview Table
        sb.AppendLine("## Repository Overview");
        sb.AppendLine();
        sb.AppendLine("| Property | Value |");
        sb.AppendLine("|----------|-------|");
        sb.AppendLine($"| Total Files | {report.Metadata.TotalFiles} |");
        sb.AppendLine($"| Total Directories | {report.Metadata.TotalDirectories} |");
        sb.AppendLine($"| Total Size | {report.Metadata.TotalSizeBytes / 1024:N0} KB |");
        sb.AppendLine($"| Languages | {string.Join(", ", report.Metadata.DetectedLanguages)} |");
        sb.AppendLine($"| Frameworks | {string.Join(", ", report.Metadata.DetectedFrameworks)} |");
        sb.AppendLine($"| Has Tests | {(report.Metadata.HasTests ? "✓ Yes" : "✗ No")} |");
        sb.AppendLine($"| Has Dockerfile | {(report.Metadata.HasDockerfile ? "✓ Yes" : "✗ No")} |");
        sb.AppendLine($"| Has CI/CD | {(report.Metadata.HasCiCd ? "✓ Yes" : "✗ No")} |");
        sb.AppendLine();

        // Executive Summary first — most useful
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.ExecutiveSummary);

        // Structure
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.StructureAnalysis);

        // Module
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.ModuleAnalysis);

        // Dependency
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.DependencyAnalysis);

        // Dead Code
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.DeadCodeAnalysis);

        // Quality
        sb.AppendLine("---");
        sb.AppendLine();
        AppendPassSection(sb, report.QualityAnalysis);

        // Timing
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Analysis Timing");
        sb.AppendLine();
        AppendTiming(sb, report.StructureAnalysis);
        AppendTiming(sb, report.ModuleAnalysis);
        AppendTiming(sb, report.DependencyAnalysis);
        AppendTiming(sb, report.DeadCodeAnalysis);
        AppendTiming(sb, report.QualityAnalysis);
        AppendTiming(sb, report.ExecutiveSummary);
        sb.AppendLine($"- **Total**: {report.TotalDuration.TotalSeconds:F1}s");

        return sb.ToString();
    }

    private static void AppendPassSection(StringBuilder sb, PassResult pass)
    {
        sb.AppendLine($"## {pass.PassName}");
        sb.AppendLine();
        if (pass.Success)
        {
            sb.AppendLine(pass.Content);
        }
        else
        {
            sb.AppendLine($"> ⚠️ **Pass Failed**: {pass.ErrorMessage}");
        }
        sb.AppendLine();
    }

    private static void AppendTiming(StringBuilder sb, PassResult pass)
    {
        var icon = pass.Success ? "✓" : "✗";
        sb.AppendLine($"- {icon} {pass.PassName}: {pass.Duration.TotalSeconds:F1}s");
    }
}
