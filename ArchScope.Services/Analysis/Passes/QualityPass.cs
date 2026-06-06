using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class QualityPass : AnalysisPassBase
{
    public QualityPass(IAiClient aiClient, ILogger<QualityPass> logger) : base(aiClient, logger) { }

    public override string PassName => "Code Quality Analysis";
    protected override int MaxTokens => 5000;

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are performing a code quality analysis on the largest source files in a repository. " +
        "Assess naming, design patterns, maintainability, error handling, and async usage. Be specific — name files and line patterns.";

    protected override string BuildUserMessage(ChunkContext context) =>
        $"""
        ## Repository File Tree
        {context.FileTreeText}

        ## Top 20 Largest Source Files
        {FormatFiles(context.RelevantFiles)}

        ---

        Analyze the code quality of these files. Provide the following sections:

        ### Naming and Consistency
        Are names clear, consistent, and meaningful? Identify specific naming problems or good patterns.

        ### Design Patterns and Anti-Patterns
        Identify design patterns in use (good) and anti-patterns (bad). Name specific files where you see them.

        ### Maintainability Red Flags
        Tight coupling, oversized methods, deep nesting, God classes, magic numbers/strings. Be specific about which files.

        ### Error Handling
        How are exceptions handled? Are there swallowed exceptions, missing error paths, or inconsistent error strategies?

        ### Async/Await Usage
        Are async patterns used correctly? Look for sync-over-async, missing ConfigureAwait, fire-and-forget without handling, etc.

        ### Overall Maintainability Assessment
        Rating: **High** / **Medium** / **Low** / **Critical** with a one-paragraph justification.

        ### Top 5 Actionable Improvements
        Ordered list of the highest-impact improvements. Each must be specific (reference file names), actionable, and explain the payoff.
        """;
}
