using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class ModulePass : AnalysisPassBase
{
    public ModulePass(IAiClient aiClient, ILogger<ModulePass> logger) : base(aiClient, logger) { }

    public override string PassName => "Module Analysis";

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are performing a module-by-module analysis of a software repository. " +
        "Analyze each significant module/folder's responsibilities, design quality, and concerns.";

    protected override string BuildUserMessage(ChunkContext context) =>
        $"""
        ## Repository File Tree
        {context.FileTreeText}

        ## Representative Files Per Module
        {FormatFiles(context.RelevantFiles)}

        ---

        Analyze each significant module in this repository. For each important folder/module, provide:

        ### Module: [Name]
        - **Responsibility**: What this module is supposed to do
        - **Key Components**: The main classes/files and their roles
        - **Design Assessment**: Is the module well-designed? Cohesive? Following good patterns?
        - **Concerns**: Any issues with this module's design, scope, or implementation

        After covering individual modules, provide:

        ### Cross-Module Observations
        - What works well across modules (if anything genuinely does)
        - What doesn't work — architectural mismatches, leaking abstractions, missing modules
        - Whether the module boundaries make sense for the stated architecture
        """;
}
