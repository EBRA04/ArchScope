using System.Diagnostics;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class SummaryPass : AnalysisPassBase
{
    public SummaryPass(IAiClient aiClient, ILogger<SummaryPass> logger) : base(aiClient, logger) { }

    public override string PassName => "Executive Summary";
    protected override int MaxTokens => 5000;

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are writing an executive summary of a full architectural analysis. " +
        "Synthesize findings from all analysis passes into a clear, honest, actionable overview. " +
        "Do not repeat everything — distill the most important insights.";

    protected override string BuildUserMessage(ChunkContext context) =>
        throw new InvalidOperationException("SummaryPass uses RunSummaryAsync, not RunAsync.");

    public async Task<PassResult> RunSummaryAsync(
        string projectName,
        RepoMetadata metadata,
        PassResult structureResult,
        PassResult moduleResult,
        PassResult dependencyResult,
        PassResult deadCodeResult,
        PassResult qualityResult,
        CancellationToken ct = default)
    {
        Logger.LogInformation("Starting pass: {PassName}", PassName);
        var sw = Stopwatch.StartNew();

        try
        {
            var userMessage = BuildSummaryMessage(projectName, metadata, structureResult, moduleResult, dependencyResult, deadCodeResult, qualityResult);
            var content = await AiClient.CompleteAsync(SystemPrompt, userMessage, MaxTokens, ct);
            sw.Stop();

            Logger.LogInformation("Completed pass: {PassName} in {Duration:F1}s", PassName, sw.Elapsed.TotalSeconds);

            return new PassResult
            {
                PassName = PassName,
                Content = content,
                Success = true,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            Logger.LogError(ex, "Pass failed: {PassName}", PassName);

            return new PassResult
            {
                PassName = PassName,
                Content = string.Empty,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = sw.Elapsed
            };
        }
    }

    private static string BuildSummaryMessage(
        string projectName,
        RepoMetadata metadata,
        PassResult structure,
        PassResult module,
        PassResult dependency,
        PassResult deadCode,
        PassResult quality)
    {
        static string PassSection(PassResult r) =>
            r.Success ? r.Content : $"(Pass failed: {r.ErrorMessage})";

        return $"""
        ## Project: {projectName}

        ## Repository Stats
        {FormatMetadata(metadata)}

        ---

        ## Structure Analysis Results
        {PassSection(structure)}

        ---

        ## Module Analysis Results
        {PassSection(module)}

        ---

        ## Dependency Analysis Results
        {PassSection(dependency)}

        ---

        ## Dead Code Detection Results
        {PassSection(deadCode)}

        ---

        ## Code Quality Analysis Results
        {PassSection(quality)}

        ---

        Based on all the above analysis, provide a comprehensive executive summary with the following sections:

        ## Project Overview
        One paragraph: what is this project, what does it do, and what is its architecture in plain terms.

        ## Architecture Assessment
        What works architecturally and what doesn't. Be specific and honest.

        ## Critical Issues
        The most serious problems, ordered by severity. If there are none, say so directly.

        ## Maintainability Summary
        A realistic, honest assessment of how maintainable this codebase is and why.

        ## Recommended Actions
        Ordered list of recommended actions. Each tagged with [QUICK WIN], [MEDIUM EFFORT], or [MAJOR REFACTOR].

        ## What This Project Gets Right
        Genuine strengths only — do not pad this section if there is nothing notable.
        """;
    }

}
