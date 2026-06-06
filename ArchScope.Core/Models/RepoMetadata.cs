namespace ArchScope.Core.Models;

public class RepoMetadata
{
    public int TotalFiles { get; set; }
    public int TotalDirectories { get; set; }
    public long TotalSizeBytes { get; set; }
    public List<string> DetectedLanguages { get; set; } = new();
    public List<string> ConfigFiles { get; set; } = new();
    public List<string> DetectedFrameworks { get; set; } = new();
    public Dictionary<string, int> FilesByExtension { get; set; } = new();
    public bool HasTests { get; set; }
    public bool HasDockerfile { get; set; }
    public bool HasCiCd { get; set; }
}
