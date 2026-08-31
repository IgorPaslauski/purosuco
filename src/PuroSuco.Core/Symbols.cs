namespace PuroSuco.Core;

public enum SymbolKind
{
    Variable,
    Parameter,
    Function,
    Class
}

public sealed record Symbol(
    string Name,
    SymbolKind Kind,
    string TypeName,
    int Position,
    SyntaxNode Declaration);
