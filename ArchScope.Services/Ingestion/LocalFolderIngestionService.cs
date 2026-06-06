using System.Text;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Ingestion;

public class LocalFolderIngestionService : IIngestionService
{
    private readonly ILogger<LocalFolderIngestionService> _logger;

    private static readonly HashSet<string> SkipDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", ".git", ".vs", "node_modules", ".idea", "TestResults", ".nuget", ".github"
    };

    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".pdb", ".png", ".jpg", ".jpeg", ".gif", ".ico",
        ".zip", ".tar", ".gz", ".pdf", ".docx", ".xlsx", ".db", ".sqlite"
    };

    private static readonly HashSet<string> ConfigExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".json", ".xml", ".yaml", ".yml", ".toml", ".ini", ".config",
        ".csproj", ".sln", ".props", ".targets"
    };

    public LocalFolderIngestionService(ILogger<LocalFolderIngestionService> logger)
    {
        _logger = logger;
    }

    public async Task<List<RepoFile>> IngestAsync(string source, CancellationToken ct = default)
    {
        _logger.LogInformation("Ingesting local folder: {Source}", source);
        var files = new List<RepoFile>();
        var root = source.TrimEnd('/', '\\');

        var allFiles = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories);
        int count = 0;

        foreach (var fullPath in allFiles)
        {
            if (ct.IsCancellationRequested) break;
            if (count >= 500) break;

            var relativePath = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            if (ShouldSkip(relativePath)) continue;

            var info = new FileInfo(fullPath);
            var extension = info.Extension.ToLowerInvariant();
            var isBinary = BinaryExtensions.Contains(extension);
            var isConfig = ConfigExtensions.Contains(extension);
            var isTest = relativePath.Contains("test", StringComparison.OrdinalIgnoreCase)
                         || relativePath.Contains("spec", StringComparison.OrdinalIgnoreCase)
                         || relativePath.Contains("fixture", StringComparison.OrdinalIgnoreCase);

            var content = string.Empty;
            if (!isBinary && info.Length < 500 * 1024)
            {
                try
                {
                    content = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, ct);
                }
                catch { content = string.Empty; }
            }

            files.Add(new RepoFile
            {
                RelativePath = relativePath,
                FileName = info.Name,
                Extension = extension,
                Content = content,
                SizeBytes = info.Length,
                Directory = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? string.Empty,
                IsBinary = isBinary,
                IsConfig = isConfig,
                IsTestFile = isTest
            });
            count++;
        }

        _logger.LogInformation("Local ingestion complete: {Count} files", files.Count);
        return files;
    }

    private static bool ShouldSkip(string relativePath)
    {
        var parts = relativePath.Split('/', '\\');
        return parts.Any(p => SkipDirs.Contains(p));
    }
}
