namespace PuroSuco.Core;

public sealed class SemanticModel
{
    public required CompilationUnit SyntaxTree { get; init; }
    public required IReadOnlyList<SemanticDiagnostic> Diagnostics { get; init; }
    public required IReadOnlyList<Symbol> Symbols { get; init; }

    public Symbol? FindSymbol(string name) =>
        Symbols.LastOrDefault(s => s.Name.Equals(name, StringComparison.Ordinal));
}
