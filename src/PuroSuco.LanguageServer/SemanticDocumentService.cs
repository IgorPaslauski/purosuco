using PuroSuco.Core;

namespace PuroSuco.LanguageServer;

public sealed class SemanticDocumentService
{
    public SemanticModel? GetModel(string text)
    {
        try
        {
            var tree = new Parser(text).ParseCompilationUnit();
            return new SemanticAnalyzer().Analyze(tree);
        }
        catch
        {
            return null;
        }
    }
}
