using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using DocumentUri = OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri;

namespace PuroSuco.LanguageServer;

public sealed class RenameHandler : RenameHandlerBase, IPrepareRenameHandler
{
    private readonly DocumentStore _store;
    private readonly SemanticDocumentService _semantic;

    public RenameHandler(DocumentStore store, SemanticDocumentService semantic)
    {
        _store = store;
        _semantic = semantic;
    }

    protected override RenameRegistrationOptions CreateRegistrationOptions(
        RenameCapability capability,
        ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("purosuco"),
            PrepareProvider = true
        };

    public Task<RangeOrPlaceholderRange?> Handle(
        PrepareRenameParams request,
        CancellationToken cancellationToken)
    {
        var text = _store.Get(request.TextDocument.Uri.ToString());
        var word = TextUtilities.WordAt(text, request.Position);

        if (word is null)
            return Task.FromResult<RangeOrPlaceholderRange?>(null);

        var model = _semantic.GetModel(text);
        if (model?.Symbols.Any(s => s.Name == word.Value.Word) != true)
            return Task.FromResult<RangeOrPlaceholderRange?>(null);

        var range = new Range(
            TextUtilities.ToPosition(text, word.Value.Start),
            TextUtilities.ToPosition(text, word.Value.Start + word.Value.Length));

        return Task.FromResult<RangeOrPlaceholderRange?>(new RangeOrPlaceholderRange(range));
    }

    public override Task<WorkspaceEdit?> Handle(
        RenameParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var text = _store.Get(uri.ToString());
        var word = TextUtilities.WordAt(text, request.Position);

        if (word is null)
            return Task.FromResult<WorkspaceEdit?>(null);

        var model = _semantic.GetModel(text);
        if (model?.Symbols.Any(s => s.Name == word.Value.Word) != true)
            return Task.FromResult<WorkspaceEdit?>(null);

        var edits = new List<TextEdit>();
        foreach (Match match in Regex.Matches(text, $@"\b{Regex.Escape(word.Value.Word)}\b"))
        {
            edits.Add(new TextEdit
            {
                Range = new Range(
                    TextUtilities.ToPosition(text, match.Index),
                    TextUtilities.ToPosition(text, match.Index + match.Length)),
                NewText = request.NewName
            });
        }

        return Task.FromResult<WorkspaceEdit?>(new WorkspaceEdit
        {
            Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
            {
                [uri] = edits
            }
        });
    }
}
