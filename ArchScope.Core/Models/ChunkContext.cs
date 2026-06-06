namespace ArchScope.Core.Models;

public class ChunkContext
{
    public string FileTreeText { get; set; } = string.Empty;
    public RepoMetadata Metadata { get; set; } = new();
    public List<FileContent> RelevantFiles { get; set; } = new();
    public int EstimatedTokens { get; set; }
}

public class FileContent
{
    public string RelativePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
