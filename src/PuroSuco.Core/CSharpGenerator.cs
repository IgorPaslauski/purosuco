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
        _ => throw new NotSupportedException(expression.GetType().Name)
    };

    private static string WriteLiteral(LiteralExpressionSyntax literal) => literal.TypeName.ToUpperInvariant() switch
    {
        "PAPO" => $"\"{literal.Value?.ToString()?.Replace("\"", "\\\"")}\"",
        "CONFERE" => literal.Value is true ? "true" : "false",
        "TEM_NADA_AI" => "null",
        _ => literal.Value?.ToString() ?? "null"
    };

    private static string MapCall(string name) => name.ToUpperInvariant() switch
    {
        "MANDA_AI" => "Console.WriteLine",
        "FALA_TU" => "Console.ReadLine",
        _ => name
    };

    private static string MapType(string type) => type.ToUpperInvariant() switch
    {
        "NUMERO" => "int",
        "NUMERO_QUEBRADO" => "double",
        "PAPO" => "string",
        "CONFERE" => "bool",
        "SEI_LA" => "var",
        "VOLTA_NADA" => "void",
        _ => type
    };

    private void WriteModifiers(IReadOnlyList<string> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _sb.Append(modifier.ToUpperInvariant() switch
            {
                "AMOSTRADINHO" => "public ",
                "NA_MIÚDA" or "NA_MIUDA" => "private ",
                "SO_OS_DE_VERDADE" => "protected ",
                "SEMPRE_FOI_ASSIM" => "static ",
                "SO_NA_TEORIA" => "abstract ",
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
