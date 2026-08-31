using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PuroSuco.LanguageServer;

public sealed class FormattingHandler : DocumentFormattingHandlerBase
{
    private readonly DocumentStore _store;

    public FormattingHandler(DocumentStore store) => _store = store;

    protected override DocumentFormattingRegistrationOptions CreateRegistrationOptions(
        DocumentFormattingCapability capability,
        ClientCapabilities clientCapabilities) =>
        new() { DocumentSelector = TextDocumentSelector.ForLanguage("purosuco") };

    public override Task<TextEditContainer?> Handle(
        DocumentFormattingParams request,
        CancellationToken cancellationToken)
    {
        var text = _store.Get(request.TextDocument.Uri.ToString());

        string formatted;
        try
        {
            formatted = new Formatter().Format(text);
        }
        catch
        {
            return Task.FromResult<TextEditContainer?>(null);
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        var endLine = Math.Max(0, lines.Length - 1);
        var endChar = lines.Length == 0 ? 0 : lines[^1].Length;

        var edit = new TextEdit
        {
            Range = new Range(new Position(0, 0), new Position(endLine, endChar)),
            NewText = formatted
        };

        return Task.FromResult<TextEditContainer?>(new TextEditContainer(edit));
    }
}
