using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Analysis.Passes;

public class DependencyPass : AnalysisPassBase
{
    public DependencyPass(IAiClient aiClient, ILogger<DependencyPass> logger) : base(aiClient, logger) { }

    public override string PassName => "Dependency Analysis";

    protected override string SystemPrompt =>
        ArchScopeIdentity + "\n\nYou are performing a dependency analysis of a software repository. " +
        "Analyze dependency injection setup, project/package dependencies, coupling levels, and data flow.";

    protected override string BuildUserMessage(ChunkContext context) =>
        $"""
        ## Repository File Tree
        {context.FileTreeText}

        ## Dependency Injection and Wiring Files
        {FormatFiles(context.RelevantFiles)}

        ---

        Analyze the dependency structure of this repository. Provide the following sections:

        ### Dependency Injection / Wiring
        How is the DI container configured? Is the setup clean or messy? Are there Dependency Inversion Principle violations (depending on concretions instead of abstractions)?

        ### Project/Package Dependencies
        Analyze the project-to-project references and third-party package usage. Are dependency directions correct? Any suspicious or inappropriate packages?

        ### Coupling Assessment
        How tightly coupled is the system? Are concrete classes used where interfaces should be? Are there signs of spaghetti dependencies?

        ### Data Flow
        Trace how a typical request flows through the system, from entry point to response. Identify each layer it passes through.

        ### Dependency Red Flags
        List specific problems with file/class names — DIP violations, circular dependencies, god objects wired to everything, etc.
        """;
}
