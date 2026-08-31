using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using DocumentUri = OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri;

namespace PuroSuco.LanguageServer;

public sealed class CodeActionHandler : CodeActionHandlerBase
{
    private readonly DocumentStore _store;

    public CodeActionHandler(DocumentStore store) => _store = store;

    protected override CodeActionRegistrationOptions CreateRegistrationOptions(
        CodeActionCapability capability,
        ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("purosuco"),
            CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix)
        };

    public override Task<CommandOrCodeActionContainer?> Handle(
        CodeActionParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        var text = _store.Get(uri.ToString());
        var actions = new List<CommandOrCodeAction>();

        SemanticModel? model = null;
        try
        {
            var tree = new Parser(text).ParseCompilationUnit();
            model = new SemanticAnalyzer().Analyze(tree);
        }
        catch
        {
        }

        if (model is null)
            return Task.FromResult<CommandOrCodeActionContainer?>(new CommandOrCodeActionContainer(actions));

        foreach (var diagnostic in model.Diagnostics)
        {
            foreach (var fix in CodeFixProvider.GetFixes(text, diagnostic))
            {
                var edit = new TextEdit
                {
                    Range = new Range(
                        TextUtilities.ToPosition(text, fix.Start),
                        TextUtilities.ToPosition(text, fix.Start + fix.Length)),
                    NewText = fix.Replacement
                };

                actions.Add(new CodeAction
                {
                    Title = fix.Title,
                    Kind = CodeActionKind.QuickFix,
                    Edit = new WorkspaceEdit
                    {
                        Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                        {
                            [uri] = new[] { edit }
                        }
                    }
                });
            }
        }

        return Task.FromResult<CommandOrCodeActionContainer?>(new CommandOrCodeActionContainer(actions));
    }

    public override Task<CodeAction> Handle(CodeAction request, CancellationToken cancellationToken) =>
        Task.FromResult(request);
}
