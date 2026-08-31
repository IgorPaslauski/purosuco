using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace PuroSuco.LanguageServer;

public sealed class CompletionHandler : CompletionHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public CompletionHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(
        CompletionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("purosuco"),
            ResolveProvider = false
        };

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var text = _store.Get(request.TextDocument.Uri.ToString());
        var model = _semantic.GetModel(text);

        var items = new List<CompletionItem>();

        items.AddRange(MemeDictionary.Keywords.Select(kvp => new CompletionItem
        {
            Label = kvp.Key,
            Kind = CompletionItemKind.Keyword,
            Detail = $"{kvp.Key} → {kvp.Value.Equivalent}",
            Documentation = kvp.Value.Description,
            InsertText = kvp.Key
        }));

        if (model is not null)
        {
            foreach (var symbol in model.Symbols
                         .GroupBy(s => s.Name)
                         .Select(g => g.Last()))
            {
                items.Add(new CompletionItem
                {
                    Label = symbol.Name,
                    Kind = symbol.Kind switch
                    {
                        PuroSuco.Core.SymbolKind.Function => CompletionItemKind.Function,
                        PuroSuco.Core.SymbolKind.Class => CompletionItemKind.Class,
                        PuroSuco.Core.SymbolKind.Parameter => CompletionItemKind.Variable,
                        _ => CompletionItemKind.Variable
                    },
                    Detail = $"{symbol.Kind}: {symbol.TypeName}"
                });
            }
        }

        return Task.FromResult(new CompletionList(items));
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) =>
        Task.FromResult(request);
}
