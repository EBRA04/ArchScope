using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IReportService
{
    string GenerateMarkdown(AnalysisReport report);
}
