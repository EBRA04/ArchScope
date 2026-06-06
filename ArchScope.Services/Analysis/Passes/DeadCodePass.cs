using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class DeadCodePass : AnalysisPassBase
{
    public DeadCodePass(IAiClient aiClient, ILogger<DeadCodePass> logger) : base(aiClient, logger) { }

    public override string PassName => "Dead Code Detection";

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are performing dead code detection on a software repository. " +
        "Based on file signatures, identify unused files, duplicate logic, stale abstractions, and suspicious patterns. " +
        "Be honest about the limitations of static analysis — do not fabricate findings.";

    protected override string BuildUserMessage(ChunkContext context) =>
        $"""
        ## Repository File Tree
        {context.FileTreeText}

        ## Source File Signatures
        {FormatFiles(context.RelevantFiles)}

        ---

        Perform dead code detection on this codebase. Use these labels for findings:
        - [LIKELY DEAD] — Strong evidence this code is unreachable or unused
        - [SUSPICIOUS] — Warrants investigation; may be dead
        - [INVESTIGATE] — Cannot determine from signatures alone; needs manual review

        Provide the following sections:

        ### Unused/Orphaned Files
        Files that appear to have no callers, no references, or serve no evident purpose. Include label and brief reasoning.

        ### Duplicate or Redundant Logic
        Multiple files that appear to implement the same thing. Consolidation candidates.

        ### Stale Abstractions
        Interfaces with no visible implementations, empty base classes, abstract classes with no known subclasses.

        ### Suspicious Patterns
        Code that looks like it was left over from an old approach — commented-out blocks, v1/v2/old naming, unused configuration.

        ### Summary
        Count of findings by severity: Likely Dead, Suspicious, Investigate.

        Note: These findings are based on static analysis of file signatures only. Dynamic dispatch, reflection, and external callers are not visible from this analysis.
        """;
}
