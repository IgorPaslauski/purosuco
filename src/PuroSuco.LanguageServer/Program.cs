using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Server;

var server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .WithServices(services =>
        {
            services.AddSingleton<PuroSuco.LanguageServer.DocumentStore>();
            services.AddSingleton<PuroSuco.LanguageServer.DiagnosticsAnalyzer>();
            services.AddSingleton<PuroSuco.LanguageServer.SemanticDocumentService>();
        })
        .WithHandler<PuroSuco.LanguageServer.TextDocumentHandler>()
        .WithHandler<PuroSuco.LanguageServer.CompletionHandler>()
        .WithHandler<PuroSuco.LanguageServer.HoverHandler>()
        .WithHandler<PuroSuco.LanguageServer.DefinitionHandler>()
        .WithHandler<PuroSuco.LanguageServer.RenameHandler>()
        .WithHandler<PuroSuco.LanguageServer.ReferencesHandler>()
        .WithHandler<PuroSuco.LanguageServer.DocumentSymbolHandler>()
        .WithHandler<PuroSuco.LanguageServer.FormattingHandler>()
        .WithHandler<PuroSuco.LanguageServer.SemanticTokensHandler>()
        .WithHandler<PuroSuco.LanguageServer.CodeActionHandler>()
);

await server.WaitForExit;
