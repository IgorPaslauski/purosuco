using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using SymbolKind = PuroSuco.Core.SymbolKind;

namespace PuroSuco.LanguageServer;

public sealed class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    private static readonly SemanticTokenType[] Types =
    [
        SemanticTokenType.Class,
        SemanticTokenType.Function,
        SemanticTokenType.Variable,
        SemanticTokenType.Parameter
    ];

    public SemanticTokensHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("purosuco"),
            Legend = new SemanticTokensLegend
            {
                TokenTypes = new Container<SemanticTokenType>(Types),
                TokenModifiers = new Container<SemanticTokenModifier>()
            },
            Full = true,
            Range = false
        };

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(
        ITextDocumentIdentifierParams @params,
        CancellationToken cancellationToken) =>
        Task.FromResult(new SemanticTokensDocument(new SemanticTokensLegend
        {
            TokenTypes = new Container<SemanticTokenType>(Types),
            TokenModifiers = new Container<SemanticTokenModifier>()
        }));

    protected override Task Tokenize(
        SemanticTokensBuilder builder,
        ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        var uri = identifier.TextDocument.Uri;
        var text = _store.Get(uri.ToString());
        var model = _semantic.GetModel(text);

        if (model is null)
            return Task.CompletedTask;

        foreach (var symbol in model.Symbols)
        {
            var position = TextUtilities.ToPosition(text, symbol.Position);
            var tokenType = symbol.Kind switch
            {
                SymbolKind.Class => SemanticTokenType.Class,
                SymbolKind.Function => SemanticTokenType.Function,
                SymbolKind.Parameter => SemanticTokenType.Parameter,
                _ => SemanticTokenType.Variable
            };

            builder.Push(
                position.Line,
                position.Character,
                Math.Max(1, symbol.Name.Length),
                tokenType,
                Array.Empty<SemanticTokenModifier>());
        }

        return Task.CompletedTask;
    }
}
