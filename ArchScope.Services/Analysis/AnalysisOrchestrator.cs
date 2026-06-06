using System.Diagnostics;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using ArchScope.Services.Analysis.Passes;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis;

public class AnalysisOrchestrator : IAnalysisOrchestrator
{
    private readonly IChunkingService _chunkingService;
    private readonly IReportService _reportService;
    private readonly StructurePass _structurePass;
    private readonly ModulePass _modulePass;
    private readonly DependencyPass _dependencyPass;
    private readonly DeadCodePass _deadCodePass;
    private readonly QualityPass _qualityPass;
    private readonly SummaryPass _summaryPass;
    private readonly ILogger<AnalysisOrchestrator> _logger;

    public AnalysisOrchestrator(
        IChunkingService chunkingService,
        IReportService reportService,
        StructurePass structurePass,
        ModulePass modulePass,
        DependencyPass dependencyPass,
        DeadCodePass deadCodePass,
        QualityPass qualityPass,
        SummaryPass summaryPass,
        ILogger<AnalysisOrchestrator> logger)
    {
        _chunkingService = chunkingService;
        _reportService = reportService;
        _structurePass = structurePass;
        _modulePass = modulePass;
        _dependencyPass = dependencyPass;
        _deadCodePass = deadCodePass;
        _qualityPass = qualityPass;
        _summaryPass = summaryPass;
        _logger = logger;
    }

    public async Task<AnalysisReport> AnalyzeAsync(
        FileTree fileTree,
        string projectName,
        string sourceType,
        string jobId,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting analysis for job {JobId}: {ProjectName}", jobId, projectName);

        var report = new AnalysisReport
        {
            JobId = jobId,
            ProjectName = projectName,
            SourceType = sourceType,
            Metadata = fileTree.Metadata,
            GeneratedAt = DateTime.UtcNow
        };

        report.StructureAnalysis = await RunPass(
            () => _structurePass.RunAsync(_chunkingService.BuildContext(fileTree, "structure"), ct),
            "structure");

        report.ModuleAnalysis = await RunPass(
            () => _modulePass.RunAsync(_chunkingService.BuildContext(fileTree, "module"), ct),
            "module");

        report.DependencyAnalysis = await RunPass(
            () => _dependencyPass.RunAsync(_chunkingService.BuildContext(fileTree, "dependency"), ct),
            "dependency");

        report.DeadCodeAnalysis = await RunPass(
            () => _deadCodePass.RunAsync(_chunkingService.BuildContext(fileTree, "deadcode"), ct),
            "deadcode");

        report.QualityAnalysis = await RunPass(
            () => _qualityPass.RunAsync(_chunkingService.BuildContext(fileTree, "quality"), ct),
            "quality");

        report.ExecutiveSummary = await RunPass(
            () => _summaryPass.RunSummaryAsync(
                projectName, fileTree.Metadata,
                report.StructureAnalysis, report.ModuleAnalysis,
                report.DependencyAnalysis, report.DeadCodeAnalysis,
                report.QualityAnalysis, ct),
            "summary");

        sw.Stop();
        report.TotalDuration = sw.Elapsed;
        report.IsComplete = true;
        report.FullMarkdownReport = _reportService.GenerateMarkdown(report);

        _logger.LogInformation("Analysis complete for job {JobId} in {Duration:F1}s", jobId, sw.Elapsed.TotalSeconds);
        return report;
    }

    private async Task<PassResult> RunPass(Func<Task<PassResult>> passFunc, string passName)
    {
        try
        {
            return await passFunc();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in pass '{PassName}'", passName);
            return new PassResult
            {
                PassName = passName,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = TimeSpan.Zero
            };
        }
    }
}
