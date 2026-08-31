using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace PuroSuco.LanguageServer;

public sealed class HoverHandler : HoverHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public HoverHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override HoverRegistrationOptions CreateRegistrationOptions(
        HoverCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = TextDocumentSelector.ForLanguage("purosuco") };

    public override Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var text = _store.Get(request.TextDocument.Uri.ToString());
        var word = TextUtilities.WordAt(text, request.Position);
        if (word is null) return Task.FromResult<Hover?>(null);

        if (MemeDictionary.Keywords.TryGetValue(word.Value.Word, out var info))
        {
            return Task.FromResult<Hover?>(new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"**{word.Value.Word}**\\n\\nEquivale a `{info.Equivalent}`.\\n\\n{info.Description}"
                })
            });
        }

        var model = _semantic.GetModel(text);
        var symbol = model?.Symbols.LastOrDefault(s => s.Name == word.Value.Word);
        if (symbol is null) return Task.FromResult<Hover?>(null);

        return Task.FromResult<Hover?>(new Hover
        {
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = $"**{symbol.Name}**\\n\\n{symbol.Kind} · `{symbol.TypeName}`"
            })
        });
    }
}
