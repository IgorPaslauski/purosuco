namespace PuroSuco.Core;

public sealed record SemanticDiagnostic(
    string Code,
    string Title,
    string Message,
    int Position,
    int Length = 1,
    bool IsError = true);
