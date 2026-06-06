using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IFileTreeAnalyzer
{
    FileTree BuildTree(List<RepoFile> files, string rootName);
}
