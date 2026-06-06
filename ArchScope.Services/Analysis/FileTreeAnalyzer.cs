using System.Text;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;

namespace ArchScope.Services.Analysis;

public class FileTreeAnalyzer : IFileTreeAnalyzer
{
    private static readonly Dictionary<string, string> ExtensionToLanguage = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".cs", "C#" }, { ".ts", "TypeScript" }, { ".tsx", "TypeScript" },
        { ".js", "JavaScript" }, { ".jsx", "JavaScript" },
        { ".py", "Python" }, { ".go", "Go" }, { ".rs", "Rust" },
        { ".java", "Java" }, { ".kt", "Kotlin" }, { ".rb", "Ruby" },
        { ".cpp", "C++" }, { ".c", "C" }, { ".h", "C/C++" },
        { ".fs", "F#" }, { ".vb", "VB.NET" }, { ".php", "PHP" },
        { ".swift", "Swift" }
    };

    public FileTree BuildTree(List<RepoFile> files, string rootName)
    {
        var byDirectory = files
            .GroupBy(f => f.Directory)
            .ToDictionary(g => g.Key, g => g.ToList());

        var directories = files
            .Select(f => f.Directory)
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var metadata = BuildMetadata(files);
        var treeText = BuildAsciiTree(files, rootName);

        return new FileTree
        {
            RootName = rootName,
            AllFiles = files,
            ByDirectory = byDirectory,
            Directories = directories,
            Metadata = metadata,
            TreeText = treeText
        };
    }

    private static RepoMetadata BuildMetadata(List<RepoFile> files)
    {
        var languages = new HashSet<string>();
        var frameworks = new HashSet<string>();
        var configFiles = new List<string>();
        var byExtension = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (ExtensionToLanguage.TryGetValue(file.Extension, out var lang))
                languages.Add(lang);

            if (file.IsConfig)
                configFiles.Add(file.FileName);

            byExtension.TryGetValue(file.Extension, out var cnt);
            byExtension[file.Extension] = cnt + 1;
        }

        // Framework detection
        var allPaths = files.Select(f => f.RelativePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allFileNames = files.Select(f => f.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var csprojContents = files
            .Where(f => f.Extension == ".csproj")
            .Select(f => f.Content)
            .ToList();

        if (allFileNames.Contains("Program.cs") || allFileNames.Contains("appsettings.json"))
            frameworks.Add("ASP.NET Core");

        if (allFileNames.Contains("package.json"))
            frameworks.Add("Node.js");

        if (allFileNames.Contains("Dockerfile"))
            frameworks.Add("Docker");

        if (allFileNames.Contains("docker-compose.yml") || allFileNames.Contains("docker-compose.yaml"))
            frameworks.Add("Docker Compose");

        if (allPaths.Any(p => p.Contains(".github/workflows", StringComparison.OrdinalIgnoreCase)))
            frameworks.Add("GitHub Actions");

        foreach (var csproj in csprojContents)
        {
            if (csproj.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase)) frameworks.Add("EF Core");
            if (csproj.Contains("MassTransit", StringComparison.OrdinalIgnoreCase)) frameworks.Add("MassTransit");
            if (csproj.Contains("Dapper", StringComparison.OrdinalIgnoreCase)) frameworks.Add("Dapper");
        }

        return new RepoMetadata
        {
            TotalFiles = files.Count,
            TotalDirectories = files.Select(f => f.Directory).Distinct().Count(),
            TotalSizeBytes = files.Sum(f => f.SizeBytes),
            DetectedLanguages = languages.OrderBy(l => l).ToList(),
            ConfigFiles = configFiles.Distinct().OrderBy(c => c).ToList(),
            DetectedFrameworks = frameworks.OrderBy(f => f).ToList(),
            FilesByExtension = byExtension,
            HasTests = files.Any(f => f.IsTestFile),
            HasDockerfile = allFileNames.Contains("Dockerfile"),
            HasCiCd = allPaths.Any(p =>
                p.Contains(".github/workflows", StringComparison.OrdinalIgnoreCase) ||
                p.Contains(".gitlab-ci", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("azure-pipelines", StringComparison.OrdinalIgnoreCase) ||
                allFileNames.Contains("Jenkinsfile"))
        };
    }

    private static string BuildAsciiTree(List<RepoFile> files, string rootName)
    {
        var sb = new StringBuilder();
        sb.AppendLine(rootName + "/");

        // Build directory tree structure
        var tree = new SortedDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var dir = file.Directory ?? string.Empty;
            if (!tree.ContainsKey(dir))
                tree[dir] = new List<string>();
            tree[dir].Add(file.FileName);
        }

        // Render root-level files
        var rootFiles = tree.TryGetValue("", out var rf) ? rf.OrderBy(f => f).ToList() : new List<string>();
        var topDirs = tree.Keys
            .Where(k => !string.IsNullOrEmpty(k) && !k.Contains('/'))
            .OrderBy(k => k)
            .ToList();

        for (int i = 0; i < rootFiles.Count; i++)
        {
            bool lastItem = i == rootFiles.Count - 1 && topDirs.Count == 0;
            sb.AppendLine((lastItem ? "└── " : "├── ") + rootFiles[i]);
        }

        RenderDirs(sb, files, "", topDirs, "");
        return sb.ToString();
    }

    private static void RenderDirs(StringBuilder sb, List<RepoFile> allFiles, string parentDir, List<string> dirs, string indent)
    {
        for (int di = 0; di < dirs.Count; di++)
        {
            var dir = dirs[di];
            bool lastDir = di == dirs.Count - 1;
            var connector = lastDir ? "└── " : "├── ";
            var childIndent = indent + (lastDir ? "    " : "│   ");

            sb.AppendLine(indent + connector + dir + "/");

            var fullDirPath = string.IsNullOrEmpty(parentDir) ? dir : parentDir + "/" + dir;

            // Files in this directory
            var filesInDir = allFiles
                .Where(f => f.Directory == fullDirPath)
                .OrderBy(f => f.FileName)
                .ToList();

            // Subdirectories
            var subDirs = allFiles
                .Select(f => f.Directory ?? "")
                .Where(d => d.StartsWith(fullDirPath + "/", StringComparison.OrdinalIgnoreCase))
                .Select(d => d[(fullDirPath.Length + 1)..].Split('/')[0])
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            for (int fi = 0; fi < filesInDir.Count; fi++)
            {
                bool lastItem = fi == filesInDir.Count - 1 && subDirs.Count == 0;
                sb.AppendLine(childIndent + (lastItem ? "└── " : "├── ") + filesInDir[fi].FileName);
            }

            if (subDirs.Count > 0)
                RenderDirs(sb, allFiles, fullDirPath, subDirs, childIndent);
        }
    }
}
