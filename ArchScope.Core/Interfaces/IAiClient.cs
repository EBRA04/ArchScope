namespace ArchScope.Core.Interfaces;

public interface IAiClient
{
    Task<string> CompleteAsync(
        string systemPrompt,
        string userMessage,
        int maxTokens = 4096,
        CancellationToken ct = default);

    string ProviderName { get; }
}
