namespace PuroSuco.Core;

public abstract record SyntaxNode(int Position);

public sealed record CompilationUnit(
    IReadOnlyList<MemberSyntax> Members,
    int Position = 0) : SyntaxNode(Position);

public abstract record MemberSyntax(int Position) : SyntaxNode(Position);

public sealed record UsingDirectiveSyntax(
    string NamespaceName,
    int Position) : MemberSyntax(Position);

public sealed record NamespaceDeclarationSyntax(
    string Name,
    IReadOnlyList<MemberSyntax> Members,
    int Position) : MemberSyntax(Position);

public sealed record TypeDeclarationSyntax(
    string TypeKindKeyword, // TROPA, PRINT, MINI_TROPA, PAPO_RETO, CARDAPIO
    string Name,
    IReadOnlyList<MemberSyntax> Members,
    IReadOnlyList<string> Modifiers,
    int Position) : MemberSyntax(Position);

public sealed record ClassDeclarationSyntax(
    string Name,
    IReadOnlyList<MemberSyntax> Members,
    IReadOnlyList<string> Modifiers,
    int Position) : MemberSyntax(Position);

public sealed record FunctionDeclarationSyntax(
    string Name,
    string ReturnType,
    IReadOnlyList<ParameterSyntax> Parameters,
    BlockStatementSyntax Body,
    IReadOnlyList<string> Modifiers,
    int Position) : MemberSyntax(Position);

public sealed record GlobalStatementSyntax(
    StatementSyntax Statement,
    int Position) : MemberSyntax(Position);

public sealed record ParameterSyntax(
    string TypeName,
    string Name,
    int Position) : SyntaxNode(Position);

public abstract record StatementSyntax(int Position) : SyntaxNode(Position);

public sealed record BlockStatementSyntax(
    IReadOnlyList<StatementSyntax> Statements,
    int Position) : StatementSyntax(Position);

public sealed record VariableDeclarationSyntax(
    string TypeName,
    string Identifier,
    ExpressionSyntax? Initializer,
    int Position) : StatementSyntax(Position);

public sealed record AssignmentStatementSyntax(
    string Identifier,
    ExpressionSyntax Expression,
    int Position) : StatementSyntax(Position);

public sealed record ExpressionStatementSyntax(
    ExpressionSyntax Expression,
    int Position) : StatementSyntax(Position);

public sealed record IfStatementSyntax(
    ExpressionSyntax Condition,
    BlockStatementSyntax Then,
    BlockStatementSyntax? Else,
    int Position) : StatementSyntax(Position);

public sealed record WhileStatementSyntax(
    ExpressionSyntax Condition,
    BlockStatementSyntax Body,
    int Position) : StatementSyntax(Position);

public sealed record DoWhileStatementSyntax(
    BlockStatementSyntax Body,
    ExpressionSyntax Condition,
    int Position) : StatementSyntax(Position);

public sealed record ForStatementSyntax(
    StatementSyntax? Initializer,
    ExpressionSyntax? Condition,
    StatementSyntax? Increment,
    BlockStatementSyntax Body,
    int Position) : StatementSyntax(Position);

public sealed record ForeachStatementSyntax(
    string TypeName,
    string Identifier,
    ExpressionSyntax Collection,
    BlockStatementSyntax Body,
    int Position) : StatementSyntax(Position);

public sealed record TryStatementSyntax(
    BlockStatementSyntax TryBlock,
    IReadOnlyList<CatchClauseSyntax> CatchClauses,
    BlockStatementSyntax? FinallyBlock,
    int Position) : StatementSyntax(Position);

public sealed record CatchClauseSyntax(
    string? ExceptionType,
    string? Identifier,
    BlockStatementSyntax Body,
    int Position) : SyntaxNode(Position);

public sealed record ThrowStatementSyntax(
    ExpressionSyntax? Expression,
    int Position) : StatementSyntax(Position);

public sealed record ReturnStatementSyntax(
    ExpressionSyntax? Expression,
    int Position) : StatementSyntax(Position);

public sealed record BreakStatementSyntax(int Position) : StatementSyntax(Position);
public sealed record ContinueStatementSyntax(int Position) : StatementSyntax(Position);

public abstract record ExpressionSyntax(int Position) : SyntaxNode(Position);

public sealed record LiteralExpressionSyntax(
    object? Value,
    string TypeName,
    int Position) : ExpressionSyntax(Position);

public sealed record NameExpressionSyntax(
    string Identifier,
    int Position) : ExpressionSyntax(Position);

public sealed record BinaryExpressionSyntax(
    ExpressionSyntax Left,
    string Operator,
    ExpressionSyntax Right,
    int Position) : ExpressionSyntax(Position);

public sealed record CallExpressionSyntax(
    string Name,
    IReadOnlyList<ExpressionSyntax> Arguments,
    int Position) : ExpressionSyntax(Position);

public sealed record MemberAccessExpressionSyntax(
    ExpressionSyntax Target,
    string MemberName,
    int Position) : ExpressionSyntax(Position);

public sealed record AwaitExpressionSyntax(
    ExpressionSyntax Expression,
    int Position) : ExpressionSyntax(Position);

public sealed record NewExpressionSyntax(
    string TypeName,
    IReadOnlyList<ExpressionSyntax> Arguments,
    int Position) : ExpressionSyntax(Position);

