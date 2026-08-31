namespace PuroSuco.Core;

public sealed record Token(TokenKind Kind, string Text, int Position);
