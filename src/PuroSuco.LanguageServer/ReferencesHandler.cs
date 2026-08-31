using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PuroSuco.LanguageServer;

public sealed class ReferencesHandler : ReferencesHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public ReferencesHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override ReferenceRegistrationOptions CreateRegistrationOptions(
        ReferenceCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = TextDocumentSelector.ForLanguage("purosuco") };

    public override Task<LocationContainer?> Handle(
        ReferenceParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var text = _store.Get(uri.ToString());
        var word = TextUtilities.WordAt(text, request.Position);

        if (word is null)
            return Task.FromResult<LocationContainer?>(null);

        var model = _semantic.GetModel(text);
        if (model?.Symbols.Any(s => s.Name == word.Value.Word) != true)
            return Task.FromResult<LocationContainer?>(null);

        var locations = ReferenceFinder.Find(text, word.Value.Word)
            .Select(r => new Location
            {
                Uri = uri,
                Range = new Range(
                    TextUtilities.ToPosition(text, r.Position),
                    TextUtilities.ToPosition(text, r.Position + r.Length))
            });

        return Task.FromResult<LocationContainer?>(new LocationContainer(locations));
    }
}
