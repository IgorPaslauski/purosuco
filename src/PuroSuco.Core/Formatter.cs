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
            case UsingDirectiveSyntax u:
                WriteIndent();
                _sb.Append("CHAMA ").Append(u.NamespaceName).AppendLine(";");
                break;

            case NamespaceDeclarationSyntax ns:
                WriteIndent();
                _sb.Append("QUEBRADA ").Append(ns.Name).AppendLine(" {");
                _indent++;
                for (var i = 0; i < ns.Members.Count; i++)
                {
                    WriteMember(ns.Members[i]);
                    if (i < ns.Members.Count - 1) _sb.AppendLine();
                }
                _indent--;
                WriteIndent();
                _sb.AppendLine("}");
                break;

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

            case TypeDeclarationSyntax t:
                WriteIndent();
                WriteModifiers(t.Modifiers);
                _sb.Append(t.TypeKindKeyword).Append(' ').Append(t.Name).AppendLine(" {");
                _indent++;

                for (var i = 0; i < t.Members.Count; i++)
                {
                    WriteMember(t.Members[i]);
                    if (i < t.Members.Count - 1)
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

            case DoWhileStatementSyntax dw:
                WriteIndent();
                _sb.AppendLine("FAZ_PRIMEIRO {");
                _indent++;
                foreach (var s in dw.Body.Statements) WriteStatement(s);
                _indent--;
                WriteIndent();
                _sb.Append("} ENQUANTO_TANKAR ").Append(WriteExpression(dw.Condition)).AppendLine(";");
                break;

            case ForeachStatementSyntax fe:
                WriteIndent();
                _sb.Append("PRA_CADA_UM (").Append(fe.TypeName).Append(' ').Append(fe.Identifier).Append(" DENTRO_DE ").Append(WriteExpression(fe.Collection)).AppendLine(") {");
                _indent++;
                foreach (var s in fe.Body.Statements) WriteStatement(s);
                _indent--;
                WriteIndent();
                _sb.AppendLine("}");
                break;

            case TryStatementSyntax tryStmt:
                WriteIndent();
                _sb.AppendLine("VAI_DAR_BOM {");
                _indent++;
                foreach (var s in tryStmt.TryBlock.Statements) WriteStatement(s);
                _indent--;
                WriteIndent();
                _sb.AppendLine("}");

                foreach (var c in tryStmt.CatchClauses)
                {
                    WriteIndent();
                    if (c.ExceptionType is not null)
                    {
                        var id = c.Identifier is not null ? $" {c.Identifier}" : "";
                        _sb.Append("DEU_RUIM (").Append(c.ExceptionType).Append(id).AppendLine(") {");
                    }
                    else
                    {
                        _sb.AppendLine("DEU_RUIM {");
                    }
                    _indent++;
                    foreach (var s in c.Body.Statements) WriteStatement(s);
                    _indent--;
                    WriteIndent();
                    _sb.AppendLine("}");
                }

                if (tryStmt.FinallyBlock is not null)
                {
                    WriteIndent();
                    _sb.AppendLine("DE_QUALQUER_JEITO {");
                    _indent++;
                    foreach (var s in tryStmt.FinallyBlock.Statements) WriteStatement(s);
                    _indent--;
                    WriteIndent();
                    _sb.AppendLine("}");
                }
                break;

            case ThrowStatementSyntax th:
                WriteIndent();
                _sb.Append("AI_TU_ME_QUEBRA");
                if (th.Expression is not null) _sb.Append(' ').Append(WriteExpression(th.Expression));
                _sb.AppendLine(";");
                break;

            case ForStatementSyntax forStmt:
                WriteIndent();
                _sb.Append("BORA_BILL (");
                if (forStmt.Initializer is not null)
                {
                    if (forStmt.Initializer is VariableDeclarationSyntax v)
                        _sb.Append($"{v.TypeName} {v.Identifier}{(v.Initializer is not null ? $" RECEBA {WriteExpression(v.Initializer)}" : "")}; ");
                    else if (forStmt.Initializer is AssignmentStatementSyntax a)
                        _sb.Append($"{a.Identifier} RECEBA {WriteExpression(a.Expression)}; ");
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
                        _sb.Append($"{a.Identifier} RECEBA {WriteExpression(a.Expression)}");
                    else if (forStmt.Increment is ExpressionStatementSyntax es)
                        _sb.Append(WriteExpression(es.Expression));
                }
                _sb.AppendLine(") {");
                _indent++;
                foreach (var s in forStmt.Body.Statements) WriteStatement(s);
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
        MemberAccessExpressionSyntax m => $"{WriteExpression(m.Target)}.{m.MemberName}",
        AwaitExpressionSyntax a => $"PERAI {WriteExpression(a.Expression)}",
        NewExpressionSyntax nw => $"BROTOU {nw.TypeName}({string.Join(", ", nw.Arguments.Select(WriteExpression))})",
        _ => ""
    };

    private void WriteModifiers(IReadOnlyList<string> modifiers)
    {
        foreach (var m in modifiers)
            _sb.Append(m).Append(' ');
    }

    private void WriteIndent() => _sb.Append(' ', _indent * 4);
}
