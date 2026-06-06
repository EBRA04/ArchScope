using System.IO.Compression;
using System.Text;
using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Ingestion;

public class ZipIngestionService : IIngestionService
{
    private readonly ILogger<ZipIngestionService> _logger;

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

    public ZipIngestionService(ILogger<ZipIngestionService> logger)
    {
        _logger = logger;
    }

    public async Task<List<RepoFile>> IngestAsync(string source, CancellationToken ct = default)
    {
        _logger.LogInformation("Ingesting ZIP file: {Source}", source);
        var files = new List<RepoFile>();

        using var archive = ZipFile.OpenRead(source);

        // Detect and strip common root prefix (e.g. "repo-main/")
        var rootPrefix = DetectRootPrefix(archive);

        int fileCount = 0;
        foreach (var entry in archive.Entries)
        {
            if (ct.IsCancellationRequested) break;
            if (string.IsNullOrEmpty(entry.Name)) continue; // directory entry

            var relativePath = entry.FullName;
            if (!string.IsNullOrEmpty(rootPrefix) && relativePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                relativePath = relativePath[rootPrefix.Length..];

            if (ShouldSkip(relativePath)) continue;
            if (fileCount >= 500) break;

            var repoFile = await BuildRepoFileAsync(entry, relativePath, ct);
            files.Add(repoFile);
            fileCount++;
        }

        _logger.LogInformation("ZIP ingestion complete: {Count} files", files.Count);
        return files;
    }

    private static string DetectRootPrefix(ZipArchive archive)
    {
        var firstEntry = archive.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.Name));
        if (firstEntry == null) return string.Empty;

        var slash = firstEntry.FullName.IndexOf('/');
        if (slash <= 0) return string.Empty;

        var candidate = firstEntry.FullName[..(slash + 1)];
        // Verify most entries share this prefix
        var total = archive.Entries.Count(e => !string.IsNullOrEmpty(e.Name));
        var matching = archive.Entries.Count(e => e.FullName.StartsWith(candidate, StringComparison.OrdinalIgnoreCase));
        return matching > total * 0.8 ? candidate : string.Empty;
    }

    private static bool ShouldSkip(string relativePath)
    {
        var parts = relativePath.Split('/', '\\');
        return parts.Any(p => SkipDirs.Contains(p));
    }

    private static async Task<RepoFile> BuildRepoFileAsync(ZipArchiveEntry entry, string relativePath, CancellationToken ct)
    {
        var fileName = Path.GetFileName(relativePath);
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();
        var directory = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? string.Empty;
        var isBinary = BinaryExtensions.Contains(extension);
        var isConfig = ConfigExtensions.Contains(extension);
        var isTest = relativePath.Contains("test", StringComparison.OrdinalIgnoreCase)
                     || relativePath.Contains("spec", StringComparison.OrdinalIgnoreCase)
                     || relativePath.Contains("fixture", StringComparison.OrdinalIgnoreCase);

        var content = string.Empty;
        if (!isBinary && entry.Length < 500 * 1024)
        {
            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                content = await reader.ReadToEndAsync(ct);
            }
            catch
            {
                content = string.Empty;
            }
        }

        return new RepoFile
        {
            RelativePath = relativePath,
            FileName = fileName,
            Extension = extension,
            Content = content,
            SizeBytes = entry.Length,
            Directory = directory,
            IsBinary = isBinary,
            IsConfig = isConfig,
            IsTestFile = isTest
        };
    }
}
