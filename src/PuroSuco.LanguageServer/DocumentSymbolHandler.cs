using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using SymbolKind = PuroSuco.Core.SymbolKind;
using LspSymbolKind = OmniSharp.Extensions.LanguageServer.Protocol.Models.SymbolKind;

namespace PuroSuco.LanguageServer;

public sealed class DocumentSymbolHandler : DocumentSymbolHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public DocumentSymbolHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(
        DocumentSymbolCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = TextDocumentSelector.ForLanguage("purosuco") };

    public override Task<SymbolInformationOrDocumentSymbolContainer?> Handle(
        DocumentSymbolParams request,
        CancellationToken cancellationToken)
    {
        var text = _store.Get(request.TextDocument.Uri.ToString());
        var model = _semantic.GetModel(text);

        var symbols = new List<SymbolInformationOrDocumentSymbol>();

        if (model is not null)
        {
            foreach (var symbol in model.Symbols
                         .Where(s => s.Kind is SymbolKind.Class or SymbolKind.Function)
                         .GroupBy(s => (s.Name, s.Kind))
                         .Select(g => g.First()))
            {
                var start = TextUtilities.ToPosition(text, symbol.Position);
                var end = TextUtilities.ToPosition(text, symbol.Position + Math.Max(1, symbol.Name.Length));

                symbols.Add(new DocumentSymbol
                {
                    Name = symbol.Name,
                    Detail = symbol.TypeName,
                    Kind = symbol.Kind == SymbolKind.Class ? LspSymbolKind.Class : LspSymbolKind.Function,
                    Range = new Range(start, end),
                    SelectionRange = new Range(start, end)
                });
            }
        }

        return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(new SymbolInformationOrDocumentSymbolContainer(symbols));
    }
}
