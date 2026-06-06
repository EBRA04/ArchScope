using ArchScope.Core.Models;

namespace ArchScope.Core.Interfaces;

public interface IChunkingService
{
    ChunkContext BuildContext(FileTree fileTree, string passType);
}
