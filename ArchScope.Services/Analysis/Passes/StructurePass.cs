using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class StructurePass : AnalysisPassBase
{
    public StructurePass(IAiClient aiClient, ILogger<StructurePass> logger) : base(aiClient, logger) { }

    public override string PassName => "Structure Analysis";

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are performing a structural analysis of a software repository. " +
        "Analyze the file tree, configuration files, and entry points to identify the architecture pattern, " +
        "project organization, technology stack, and structural issues.";

    protected override string BuildUserMessage(ChunkContext context) =>
        $"""
        ## Repository File Tree
        {context.FileTreeText}

        ## Repository Stats
        {FormatMetadata(context.Metadata)}

        ## Key Configuration and Entry Point Files
        {FormatFiles(context.RelevantFiles)}

        ---

        Perform a structural analysis of this repository. Provide the following sections:

        ### Architecture Pattern
        Identify the architecture pattern (e.g. Clean Architecture, Layered, Hexagonal, MVC, Microservices, Monolith). State your confidence level and the specific evidence from the file tree that supports your conclusion.

        ### Project Organization Assessment
        Is the folder structure logical and consistent? Are concerns properly separated? Where is the organization strong or weak?

        ### Technology Stack
        List the specific technologies, frameworks, and versions visible in configuration files.

        ### Entry Points and System Boundaries
        Identify application entry points, API surfaces, and where the system interacts with the outside world.

        ### Structural Red Flags
        List specific structural problems (naming inconsistencies, misplaced files, missing layers, circular project references, etc.). If none exist, say so explicitly.
        """;
}
