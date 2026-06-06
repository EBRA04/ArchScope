using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;

namespace ArchScope.Services.Analysis;

public class ChunkingService : IChunkingService
{
    private const int MaxChars = 16_000; // ~4k tokens — fits within free-tier TPM limits (12k/min)

    public ChunkContext BuildContext(FileTree fileTree, string passType)
    {
        var selectedFiles = passType.ToLowerInvariant() switch
        {
            "structure" => SelectStructureFiles(fileTree),
            "module" => SelectModuleFiles(fileTree),
            "dependency" => SelectDependencyFiles(fileTree),
            "deadcode" => SelectDeadCodeFiles(fileTree),
            "quality" => SelectQualityFiles(fileTree),
            _ => new List<FileContent>()
        };

        selectedFiles = TrimToTokenBudget(selectedFiles);

        return new ChunkContext
        {
            FileTreeText = fileTree.TreeText,
            Metadata = fileTree.Metadata,
            RelevantFiles = selectedFiles,
            EstimatedTokens = (fileTree.TreeText.Length + selectedFiles.Sum(f => f.Content.Length)) / 4
        };
    }

    private static List<FileContent> SelectStructureFiles(FileTree tree)
    {
        var priorityNames = new[]
        {
            ".sln", ".csproj", "Program.cs", "Startup.cs", "appsettings.json",
            "docker-compose.yml", "docker-compose.yaml", "Dockerfile", "package.json", "go.mod"
        };

        var result = new List<FileContent>();
        foreach (var name in priorityNames)
        {
            var file = tree.AllFiles.FirstOrDefault(f =>
                f.FileName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                f.Extension.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (file != null && !result.Any(r => r.RelativePath == file.RelativePath))
            {
                result.Add(new FileContent
                {
                    RelativePath = file.RelativePath,
                    Content = file.Content,
                    Reason = "Structure/entry point file"
                });
            }
        }
        return result;
    }

    private static List<FileContent> SelectModuleFiles(FileTree tree)
    {
        var result = new List<FileContent>();

        foreach (var (dir, files) in tree.ByDirectory)
        {
            if (string.IsNullOrEmpty(dir)) continue;

            var sourceFiles = files.Where(f => !f.IsBinary).ToList();
            if (!sourceFiles.Any()) continue;

            // Add interface files (up to 3)
            var interfaceFiles = sourceFiles
                .Where(f => f.Extension == ".cs" && f.FileName.Length > 1 && f.FileName.StartsWith("I", StringComparison.Ordinal) && char.IsUpper(f.FileName[1]))
                .Take(3)
                .ToList();

            foreach (var iface in interfaceFiles)
            {
                result.Add(new FileContent { RelativePath = iface.RelativePath, Content = iface.Content, Reason = "Interface definition" });
            }

            // Add one representative file
            var representative = sourceFiles
                .Where(f => f.Extension == ".cs" && !interfaceFiles.Contains(f))
                .OrderByDescending(f => f.FileName.Contains("abstract", StringComparison.OrdinalIgnoreCase) ? 2 : 0)
                .ThenByDescending(f => f.SizeBytes)
                .FirstOrDefault();

            if (representative != null && !result.Any(r => r.RelativePath == representative.RelativePath))
            {
                result.Add(new FileContent { RelativePath = representative.RelativePath, Content = representative.Content, Reason = "Module representative" });
            }
        }

        return result;
    }

    private static List<FileContent> SelectDependencyFiles(FileTree tree)
    {
        var result = new List<FileContent>();
        var diKeywords = new[] { "DependencyInjection", "ServiceCollection", "Extension", "Module" };

        foreach (var file in tree.AllFiles)
        {
            if (file.IsBinary) continue;

            bool isDi = file.FileName.Equals("Program.cs", StringComparison.OrdinalIgnoreCase)
                || file.FileName.Equals("Startup.cs", StringComparison.OrdinalIgnoreCase)
                || file.Extension == ".csproj"
                || (file.Extension == ".cs" && diKeywords.Any(k => file.FileName.Contains(k, StringComparison.OrdinalIgnoreCase)));

            if (isDi)
                result.Add(new FileContent { RelativePath = file.RelativePath, Content = file.Content, Reason = "DI/wiring file" });
        }

        return result;
    }

    private static List<FileContent> SelectDeadCodeFiles(FileTree tree)
    {
        var result = new List<FileContent>();
        var signatureKeywords = new[] { "public", "private", "protected", "internal", "class", "interface", "abstract", "namespace", "using" };

        foreach (var file in tree.AllFiles.Where(f => !f.IsBinary && !f.IsTestFile))
        {
            var content = file.Content;
            if (content.Length > 5000)
            {
                // Extract only signature lines
                var lines = content.Split('\n');
                var signatureLines = lines
                    .Where(l => signatureKeywords.Any(k => l.TrimStart().StartsWith(k, StringComparison.Ordinal)))
                    .ToList();
                content = string.Join('\n', signatureLines);
            }

            result.Add(new FileContent { RelativePath = file.RelativePath, Content = content, Reason = "Source file signatures" });
        }

        return result;
    }

    private static List<FileContent> SelectQualityFiles(FileTree tree)
    {
        return tree.AllFiles
            .Where(f => !f.IsBinary && !f.IsTestFile && !string.IsNullOrEmpty(f.Content))
            .OrderByDescending(f => f.SizeBytes)
            .Take(20)
            .Select(f => new FileContent { RelativePath = f.RelativePath, Content = f.Content, Reason = "Large source file for quality review" })
            .ToList();
    }

    private static List<FileContent> TrimToTokenBudget(List<FileContent> files)
    {
        var result = new List<FileContent>();
        int totalChars = 0;

        foreach (var file in files)
        {
            var remaining = MaxChars - totalChars;
            if (remaining <= 0) break;

            if (file.Content.Length <= remaining)
            {
                result.Add(file);
                totalChars += file.Content.Length;
            }
            else
            {
                result.Add(new FileContent
                {
                    RelativePath = file.RelativePath,
                    Content = file.Content[..remaining] + "\n... [truncated]",
                    Reason = file.Reason
                });
                break;
            }
        }

        return result;
    }
}
