using System.Diagnostics;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public abstract class AnalysisPassBase : IAnalysisPass
{
    protected readonly IAiClient AiClient;
    protected readonly ILogger Logger;

    public abstract string PassName { get; }
    protected abstract string SystemPrompt { get; }
    protected abstract string BuildUserMessage(ChunkContext context);
    protected virtual int MaxTokens => 2000;

    protected AnalysisPassBase(IAiClient aiClient, ILogger logger)
    {
        AiClient = aiClient;
        Logger = logger;
    }

    public async Task<PassResult> RunAsync(ChunkContext context, CancellationToken ct = default)
    {
        Logger.LogInformation("Starting pass: {PassName}", PassName);
        var sw = Stopwatch.StartNew();

        try
        {
            var userMessage = BuildUserMessage(context);
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

    protected static string ArchScopeIdentity =>
        "You are ArchScope, a senior software architect performing repository analysis. " +
        "You are analytical, direct, and technically precise. You do not give generic praise or vague feedback. " +
        "If something is poorly designed, you say so clearly and explain why. " +
        "You do not invent details that are not present in the provided files.";

    protected static string FormatMetadata(RepoMetadata m) =>
        $"Files: {m.TotalFiles} | Directories: {m.TotalDirectories} | " +
        $"Size: {m.TotalSizeBytes / 1024:N0} KB | " +
        $"Languages: {string.Join(", ", m.DetectedLanguages)} | " +
        $"Frameworks: {string.Join(", ", m.DetectedFrameworks)} | " +
        $"Has Tests: {m.HasTests} | Has Dockerfile: {m.HasDockerfile} | Has CI/CD: {m.HasCiCd}";

    protected static string FormatFiles(List<FileContent> files)
    {
        if (!files.Any()) return "(no files selected)";

        return string.Join("\n\n", files.Select(f =>
            $"=== {f.RelativePath} ===\n{f.Content}"));
    }
}
