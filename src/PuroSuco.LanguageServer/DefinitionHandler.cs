using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PuroSuco.LanguageServer;

public sealed class DefinitionHandler : DefinitionHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public DefinitionHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override DefinitionRegistrationOptions CreateRegistrationOptions(
        DefinitionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = TextDocumentSelector.ForLanguage("purosuco") };

    public override Task<LocationOrLocationLinks?> Handle(
        DefinitionParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var text = _store.Get(uri.ToString());
        var word = TextUtilities.WordAt(text, request.Position);
        if (word is null) return Task.FromResult<LocationOrLocationLinks?>(null);

        var model = _semantic.GetModel(text);
        var symbol = model?.Symbols.LastOrDefault(s => s.Name == word.Value.Word);
        if (symbol is null) return Task.FromResult<LocationOrLocationLinks?>(null);

        var start = TextUtilities.ToPosition(text, symbol.Position);
        var end = TextUtilities.ToPosition(text, symbol.Position + Math.Max(1, symbol.Name.Length));

        var location = new Location
        {
            Uri = uri,
            Range = new Range(start, end)
        };

        return Task.FromResult<LocationOrLocationLinks?>(new LocationOrLocationLinks(location));
    }
}
