using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using PuroSuco.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PuroSuco.LanguageServer;

public sealed class DiagnosticsAnalyzer
{
    public IEnumerable<Diagnostic> Analyze(string text)
    {
        SemanticModel? model = null;
        PuroSucoException? parseError = null;

        try
        {
            var tree = new Parser(text).ParseCompilationUnit();
            model = new SemanticAnalyzer().Analyze(tree);
        }
        catch (PuroSucoException ex)
        {
            parseError = ex;
        }

        if (parseError is not null)
        {
            yield return Create(text, parseError.Position, 1, DiagnosticSeverity.Error, parseError.Code, $"{parseError.MemeTitle} {parseError.Message}");
            yield break;
        }

        if (model is not null)
        {
            foreach (var diagnostic in model.Diagnostics)
            {
                yield return Create(
                    text,
                    diagnostic.Position,
                    Math.Max(1, diagnostic.Length),
                    diagnostic.IsError ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning,
                    diagnostic.Code,
                    $"{diagnostic.Title}. {diagnostic.Message}");
            }
        }
    }

    private static Diagnostic Create(string text, int start, int length, DiagnosticSeverity severity, string code, string message)
    {
        return new Diagnostic
        {
            Range = new Range(
                TextUtilities.ToPosition(text, start),
                TextUtilities.ToPosition(text, Math.Min(text.Length, start + length))),
            Severity = severity,
            Code = code,
            Source = "PuroSuco",
            Message = message
        };
    }
}
