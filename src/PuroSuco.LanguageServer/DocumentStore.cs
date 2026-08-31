using System.Collections.Concurrent;

namespace PuroSuco.LanguageServer;

public sealed class DocumentStore
{
    private readonly ConcurrentDictionary<string, string> _documents = new();

    public void Set(string uri, string text) => _documents[uri] = text;
    public string Get(string uri) => _documents.TryGetValue(uri, out var text) ? text : string.Empty;
    public void Remove(string uri) => _documents.TryRemove(uri, out _);
}
