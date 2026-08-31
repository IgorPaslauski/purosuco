using System.Text;

namespace PuroSuco.Core;

public sealed class Formatter
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    public string Format(string source)
    {
        var tree = new Parser(source).ParseCompilationUnit();
        _sb.Clear();
        _indent = 0;

        for (var i = 0; i < tree.Members.Count; i++)
        {
            WriteMember(tree.Members[i]);

            if (i < tree.Members.Count - 1)
                _sb.AppendLine();
        }

        return _sb.ToString().TrimEnd() + Environment.NewLine;
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
                _sb.Append("TROPA ").Append(c.Name).AppendLine(" {");
                _indent++;

                for (var i = 0; i < c.Members.Count; i++)
                {
                    WriteMember(c.Members[i]);
                    if (i < c.Members.Count - 1)
                        _sb.AppendLine();
                }

                _indent--;
                WriteIndent();
                _sb.AppendLine("}");
                break;

            case FunctionDeclarationSyntax f:
                WriteIndent();
                WriteModifiers(f.Modifiers);
                _sb.Append(f.ReturnType).Append(' ').Append(f.Name).Append('(');
                _sb.Append(string.Join(", ", f.Parameters.Select(p => $"{p.TypeName} {p.Name}")));
                _sb.AppendLine(") {");
                _indent++;

                foreach (var s in f.Body.Statements)
                    WriteStatement(s);

                _indent--;
                WriteIndent();
                _sb.AppendLine("}");
                break;
        }
    }

    private void WriteStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case VariableDeclarationSyntax v:
                WriteIndent();
                _sb.Append(v.TypeName).Append(' ').Append(v.Identifier);
                if (v.Initializer is not null)
                    _sb.Append(" RECEBA ").Append(WriteExpression(v.Initializer));
                _sb.AppendLine(";");
                break;

            case AssignmentStatementSyntax a:
                WriteIndent();
                _sb.Append(a.Identifier).Append(" RECEBA ").Append(WriteExpression(a.Expression)).AppendLine(";");
                break;

            case ExpressionStatementSyntax e:
                WriteIndent();
                _sb.Append(WriteExpression(e.Expression)).AppendLine(";");
                break;

            case IfStatementSyntax i:
                WriteIndent();
                _sb.Append("TA_CERTO_ISSO ").Append(WriteExpression(i.Condition)).AppendLine(" {");
                _indent++;
                foreach (var s in i.Then.Statements) WriteStatement(s);
                _indent--;
                WriteIndent();
                _sb.AppendLine("}");

                if (i.Else is not null)
                {
                    WriteIndent();
                    _sb.AppendLine("NAO_TA_NAO {");
                    _indent++;
                    foreach (var s in i.Else.Statements) WriteStatement(s);
                    _indent--;
                    WriteIndent();
                    _sb.AppendLine("}");
                }
                break;

            case WhileStatementSyntax w:
                WriteIndent();
                _sb.Append("ENQUANTO_TANKAR ").Append(WriteExpression(w.Condition)).AppendLine(" {");
                _indent++;
                foreach (var s in w.Body.Statements) WriteStatement(s);
                _indent--;
                WriteIndent();
                _sb.AppendLine("}");
                break;

            case ReturnStatementSyntax r:
                WriteIndent();
                _sb.Append("TOMA");
                if (r.Expression is not null)
                    _sb.Append(' ').Append(WriteExpression(r.Expression));
                _sb.AppendLine(";");
                break;

            case BreakStatementSyntax:
                WriteIndent(); _sb.AppendLine("CHEGA;");
                break;

            case ContinueStatementSyntax:
                WriteIndent(); _sb.AppendLine("SEGUE_O_JOGO;");
                break;
        }
    }

    private string WriteExpression(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax l => l.TypeName.ToUpperInvariant() switch
        {
            "PAPO" => $"\"{l.Value?.ToString()?.Replace("\"", "\\\"")}\"",
            "CONFERE" => l.Value is true ? "CONFIA" : "CONFIA_NAO",
            "TEM_NADA_AI" => "TEM_NADA_AI",
            _ => l.Value?.ToString() ?? "TEM_NADA_AI"
        },
        NameExpressionSyntax n => n.Identifier,
        BinaryExpressionSyntax b => $"{WriteExpression(b.Left)} {b.Operator} {WriteExpression(b.Right)}",
        CallExpressionSyntax c => $"{c.Name}({string.Join(", ", c.Arguments.Select(WriteExpression))})",
        _ => ""
    };

    private void WriteModifiers(IReadOnlyList<string> modifiers)
    {
        foreach (var m in modifiers)
            _sb.Append(m).Append(' ');
    }

    private void WriteIndent() => _sb.Append(' ', _indent * 4);
}
