using System.Text;

namespace PuroSuco.Core;

public sealed class CSharpGenerator
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    public string Generate(CompilationUnit unit)
    {
        _sb.Clear();
        _indent = 0;
        _sb.AppendLine("using System;");
        _sb.AppendLine();

        foreach (var member in unit.Members)
            WriteMember(member);

        return _sb.ToString();
    }

    private void WriteMember(MemberSyntax member)
    {
        switch (member)
        {
            case UsingDirectiveSyntax u:
                WriteIndent();
                _sb.Append("using ").Append(u.NamespaceName).AppendLine(";");
                break;

            case NamespaceDeclarationSyntax ns:
                WriteIndent();
                _sb.Append("namespace ").Append(ns.Name).AppendLine();
                WriteOpenBrace();
                foreach (var child in ns.Members) WriteMember(child);
                WriteCloseBrace();
                break;

            case GlobalStatementSyntax g:
                WriteStatement(g.Statement);
                break;

            case ClassDeclarationSyntax c:
                WriteIndent();
                WriteModifiers(c.Modifiers);
                _sb.Append("class ").Append(c.Name).AppendLine();
                WriteOpenBrace();

                foreach (var child in c.Members)
                    WriteMember(child);

                WriteCloseBrace();
                break;

            case TypeDeclarationSyntax t:
                WriteIndent();
                WriteModifiers(t.Modifiers);
                var typeKind = Keywords.Normalize(t.TypeKindKeyword) switch
                {
                    "PRINT" => "record",
                    "CARDAPIO" => "enum",
                    "MINI_TROPA" => "struct",
                    "PAPO_RETO" => "interface",
                    _ => "class"
                };
                _sb.Append(typeKind).Append(' ').Append(t.Name).AppendLine();
                WriteOpenBrace();

                foreach (var child in t.Members)
                    WriteMember(child);

                WriteCloseBrace();
                break;

            case FunctionDeclarationSyntax f:
                WriteIndent();
                WriteModifiers(f.Modifiers);
                _sb.Append(MapType(f.ReturnType)).Append(' ').Append(f.Name).Append('(');
                _sb.Append(string.Join(", ", f.Parameters.Select(p => $"{MapType(p.TypeName)} {p.Name}")));
                _sb.AppendLine(")");
                WriteBlock(f.Body);
                break;
        }
    }

    private void WriteStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case VariableDeclarationSyntax v:
                WriteIndent();
                _sb.Append(MapType(v.TypeName)).Append(' ').Append(v.Identifier);
                if (v.Initializer is not null)
                    _sb.Append(" = ").Append(WriteExpression(v.Initializer));
                _sb.AppendLine(";");
                break;

            case AssignmentStatementSyntax a:
                WriteIndent();
                _sb.Append(a.Identifier).Append(" = ").Append(WriteExpression(a.Expression)).AppendLine(";");
                break;

            case ExpressionStatementSyntax e:
                WriteIndent();
                _sb.Append(WriteExpression(e.Expression)).AppendLine(";");
                break;

            case IfStatementSyntax i:
                WriteIndent();
                _sb.Append("if (").Append(WriteExpression(i.Condition)).AppendLine(")");
                WriteBlock(i.Then);
                if (i.Else is not null)
                {
                    WriteIndent();
                    _sb.AppendLine("else");
                    WriteBlock(i.Else);
                }
                break;

            case WhileStatementSyntax w:
                WriteIndent();
                _sb.Append("while (").Append(WriteExpression(w.Condition)).AppendLine(")");
                WriteBlock(w.Body);
                break;

            case DoWhileStatementSyntax dw:
                WriteIndent();
                _sb.AppendLine("do");
                WriteBlock(dw.Body);
                WriteIndent();
                _sb.Append("while (").Append(WriteExpression(dw.Condition)).AppendLine(");");
                break;

            case ForeachStatementSyntax fe:
                WriteIndent();
                _sb.Append("foreach (").Append(MapType(fe.TypeName)).Append(' ').Append(fe.Identifier).Append(" in ").Append(WriteExpression(fe.Collection)).AppendLine(")");
                WriteBlock(fe.Body);
                break;

            case TryStatementSyntax tryStmt:
                WriteIndent();
                _sb.AppendLine("try");
                WriteBlock(tryStmt.TryBlock);

                foreach (var c in tryStmt.CatchClauses)
                {
                    WriteIndent();
                    if (c.ExceptionType is not null)
                    {
                        var exId = c.Identifier is not null ? $" {c.Identifier}" : "";
                        _sb.Append("catch (").Append(MapType(c.ExceptionType)).Append(exId).AppendLine(")");
                    }
                    else
                    {
                        _sb.AppendLine("catch");
                    }
                    WriteBlock(c.Body);
                }

                if (tryStmt.FinallyBlock is not null)
                {
                    WriteIndent();
                    _sb.AppendLine("finally");
                    WriteBlock(tryStmt.FinallyBlock);
                }
                break;

            case ThrowStatementSyntax th:
                WriteIndent();
                _sb.Append("throw");
                if (th.Expression is not null) _sb.Append(' ').Append(WriteExpression(th.Expression));
                _sb.AppendLine(";");
                break;

            case ForStatementSyntax forStmt:
                WriteIndent();
                _sb.Append("for (");
                if (forStmt.Initializer is not null)
                {
                    if (forStmt.Initializer is VariableDeclarationSyntax v)
                        _sb.Append($"{MapType(v.TypeName)} {v.Identifier}{(v.Initializer is not null ? $" = {WriteExpression(v.Initializer)}" : "")}; ");
                    else if (forStmt.Initializer is AssignmentStatementSyntax a)
                        _sb.Append($"{a.Identifier} = {WriteExpression(a.Expression)}; ");
                    else if (forStmt.Initializer is ExpressionStatementSyntax es)
                        _sb.Append($"{WriteExpression(es.Expression)}; ");
                }
                else
                {
                    _sb.Append("; ");
                }

                if (forStmt.Condition is not null)
                    _sb.Append(WriteExpression(forStmt.Condition));
                _sb.Append("; ");

                if (forStmt.Increment is not null)
                {
                    if (forStmt.Increment is AssignmentStatementSyntax a)
                        _sb.Append($"{a.Identifier} = {WriteExpression(a.Expression)}");
                    else if (forStmt.Increment is ExpressionStatementSyntax es)
                        _sb.Append(WriteExpression(es.Expression));
                }
                _sb.AppendLine(")");
                WriteBlock(forStmt.Body);
                break;

            case ReturnStatementSyntax r:
                WriteIndent();
                _sb.Append("return");
                if (r.Expression is not null) _sb.Append(' ').Append(WriteExpression(r.Expression));
                _sb.AppendLine(";");
                break;

            case BreakStatementSyntax:
                WriteIndent(); _sb.AppendLine("break;");
                break;

            case ContinueStatementSyntax:
                WriteIndent(); _sb.AppendLine("continue;");
                break;
        }
    }

    private void WriteBlock(BlockStatementSyntax block)
    {
        WriteOpenBrace();
        foreach (var statement in block.Statements)
            WriteStatement(statement);
        WriteCloseBrace();
    }

    private string WriteExpression(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax l => WriteLiteral(l),
        NameExpressionSyntax n => n.Identifier,
        BinaryExpressionSyntax b => $"{WriteExpression(b.Left)} {b.Operator} {WriteExpression(b.Right)}",
        CallExpressionSyntax c => $"{MapCall(c.Name)}({string.Join(", ", c.Arguments.Select(WriteExpression))})",
        MemberAccessExpressionSyntax m => $"{WriteExpression(m.Target)}.{m.MemberName}",
        AwaitExpressionSyntax a => $"await {WriteExpression(a.Expression)}",
        NewExpressionSyntax nw => $"new {MapType(nw.TypeName)}({string.Join(", ", nw.Arguments.Select(WriteExpression))})",
        _ => throw new NotSupportedException(expression.GetType().Name)
    };

    private static string WriteLiteral(LiteralExpressionSyntax literal) => Keywords.Normalize(literal.TypeName) switch
    {
        "PAPO" => $"\"{literal.Value?.ToString()?.Replace("\"", "\\\"")}\"",
        "CONFERE" => literal.Value is true ? "true" : "false",
        "TEM_NADA_AI" => "null",
        "NUMERO_QUEBRADO" => Convert.ToString(literal.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "0.0",
        _ => literal.Value?.ToString() ?? "null"
    };

    private static string MapCall(string name) => Keywords.Normalize(name) switch
    {
        "MANDA_AI" => "Console.WriteLine",
        "SOLTA_AI" => "Console.Write",
        "FALA_TU" => "Console.ReadLine",
        _ => name
    };

    private static string MapType(string type) => Keywords.Normalize(type) switch
    {
        "NUMERO" => "int",
        "NUMERO_QUEBRADO" => "double",
        "NUMERO_BRUTO" => "long",
        "GRANA" => "decimal",
        "LETRA" => "char",
        "PAPO" => "string",
        "CONFERE" => "bool",
        "QUALQUER_COISA" => "object",
        "SEI_LA" => "var",
        "VAI_NA_FE" => "dynamic",
        "VOLTA_NADA" => "void",
        _ => type
    };

    private void WriteModifiers(IReadOnlyList<string> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _sb.Append(Keywords.Normalize(modifier) switch
            {
                "AMOSTRADINHO" => "public ",
                "NA_MIUDA" => "private ",
                "SO_OS_DE_VERDADE" => "protected ",
                "SO_ENTRE_NOS" => "internal ",
                "SEMPRE_FOI_ASSIM" => "static ",
                "SO_NA_TEORIA" => "abstract ",
                "SO_OLHA_NAO_TOCA" => "readonly ",
                "LACRADO" => "sealed ",
                "FICA_A_VONTADE" => "virtual ",
                "ASSUME_A_RESPONSA" => "override ",
                "NAO_MEXE" => "const ",
                _ => ""
            });
        }
    }

    private void WriteOpenBrace()
    {
        WriteIndent();
        _sb.AppendLine("{");
        _indent++;
    }

    private void WriteCloseBrace()
    {
        _indent--;
        WriteIndent();
        _sb.AppendLine("}");
    }

    private void WriteIndent() => _sb.Append(' ', _indent * 4);
}
