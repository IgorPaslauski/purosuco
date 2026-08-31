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
        SemanticTokenType.Keyword,
        SemanticTokenType.Type,
        SemanticTokenType.Class,
        SemanticTokenType.Function,
        SemanticTokenType.Variable,
        SemanticTokenType.Parameter,
        SemanticTokenType.String,
        SemanticTokenType.Number,
        SemanticTokenType.Operator,
        SemanticTokenType.Comment
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
        if (string.IsNullOrEmpty(text))
            return Task.CompletedTask;

        IReadOnlyList<Token> tokens;
        try
        {
            tokens = new Lexer(text).Lex();
        }
        catch
        {
            return Task.CompletedTask;
        }

        var model = _semantic.GetModel(text);
        var symbolMap = model?.Symbols
            .ToLookup(s => s.Position, s => s) ?? null;

        foreach (var token in tokens)
        {
            if (token.Kind == TokenKind.EndOfFile || token.Text.Length == 0)
                continue;

            var position = TextUtilities.ToPosition(text, token.Position);
            var tokenType = ClassifyToken(token, symbolMap);

            if (tokenType is not null)
            {
                builder.Push(
                    position.Line,
                    position.Character,
                    token.Text.Length,
                    tokenType.Value,
                    Array.Empty<SemanticTokenModifier>());
            }
        }

        return Task.CompletedTask;
    }

    private static SemanticTokenType? ClassifyToken(Token token, ILookup<int, Symbol>? symbols)
    {
        if (symbols != null && symbols.Contains(token.Position))
        {
            var symbol = symbols[token.Position].FirstOrDefault();
            if (symbol != null)
            {
                return symbol.Kind switch
                {
                    SymbolKind.Class => SemanticTokenType.Class,
                    SymbolKind.Function => SemanticTokenType.Function,
                    SymbolKind.Parameter => SemanticTokenType.Parameter,
                    _ => SemanticTokenType.Variable
                };
            }
        }

        if (token.Kind == TokenKind.Keyword)
        {
            var t = token.Text.ToUpperInvariant();
            if (t is "NUMERO" or "NUMERO_QUEBRADO" or "PAPO" or "CONFERE" or "SEI_LA" or "VOLTA_NADA" or "TROPA")
                return SemanticTokenType.Type;

            return SemanticTokenType.Keyword;
        }

        return token.Kind switch
        {
            TokenKind.Number => SemanticTokenType.Number,
            TokenKind.String => SemanticTokenType.String,
            TokenKind.Operator => SemanticTokenType.Operator,
            TokenKind.Identifier => SemanticTokenType.Variable,
            _ => null
        };
    }
}
