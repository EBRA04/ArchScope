using ArchScope.Core.Interfaces;
using ArchScope.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchScope.Services.Ingestion;

public class GitHubIngestionService : IIngestionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ZipIngestionService _zipIngestionService;
    private readonly ILogger<GitHubIngestionService> _logger;

    public GitHubIngestionService(
        IHttpClientFactory httpClientFactory,
        ZipIngestionService zipIngestionService,
        ILogger<GitHubIngestionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _zipIngestionService = zipIngestionService;
        _logger = logger;
    }

    public async Task<List<RepoFile>> IngestAsync(string source, CancellationToken ct = default)
    {
        _logger.LogInformation("Ingesting GitHub URL: {Source}", source);

        var (owner, repo, branch) = ParseGitHubUrl(source);
        _logger.LogInformation("Parsed GitHub repo: {Owner}/{Repo} @ {Branch}", owner, repo, branch ?? "auto");

        var tempFile = Path.GetTempFileName() + ".zip";
        try
        {
            await DownloadZipAsync(owner, repo, branch, tempFile, ct);
            return await _zipIngestionService.IngestAsync(tempFile, ct);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private async Task DownloadZipAsync(string owner, string repo, string? branch, string destPath, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("GitHub");
        var branchesToTry = branch != null
            ? new[] { branch }
            : new[] { "main", "master" };

        foreach (var b in branchesToTry)
        {
            var url = $"https://github.com/{owner}/{repo}/archive/refs/heads/{b}.zip";
            _logger.LogInformation("Trying branch download: {Url}", url);

            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(ct);
                    await using var file = File.Create(destPath);
                    await stream.CopyToAsync(file, ct);
                    _logger.LogInformation("Downloaded branch '{Branch}' successfully", b);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download branch '{Branch}'", b);
            }
        }

        throw new InvalidOperationException($"Could not download repository {owner}/{repo}. Tried branches: {string.Join(", ", branchesToTry)}");
    }

    private static (string owner, string repo, string? branch) ParseGitHubUrl(string url)
    {
        // Handle: https://github.com/owner/repo
        //         https://github.com/owner/repo/tree/branch
        var uri = new Uri(url.Trim());
        var segments = uri.AbsolutePath.Trim('/').Split('/');

        if (segments.Length < 2)
            throw new ArgumentException($"Invalid GitHub URL: {url}");

        var owner = segments[0];
        var repo = segments[1];
        string? branch = null;

        // /owner/repo/tree/branch-name
        if (segments.Length >= 4 && segments[2].Equals("tree", StringComparison.OrdinalIgnoreCase))
            branch = string.Join("/", segments[3..]);

        return (owner, repo, branch);
    }
}
