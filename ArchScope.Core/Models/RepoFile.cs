namespace ArchScope.Core.Models;

public class RepoFile
{
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Directory { get; set; } = string.Empty;
    public bool IsBinary { get; set; }
    public bool IsConfig { get; set; }
    public bool IsTestFile { get; set; }
}
