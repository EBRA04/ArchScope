namespace ArchScope.Core.Models;

public class FileTree
{
    public string RootName { get; set; } = string.Empty;
    public List<RepoFile> AllFiles { get; set; } = new();
    public Dictionary<string, List<RepoFile>> ByDirectory { get; set; } = new();
    public List<string> Directories { get; set; } = new();
    public RepoMetadata Metadata { get; set; } = new();
    public string TreeText { get; set; } = string.Empty;
}
