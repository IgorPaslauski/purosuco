using System.Globalization;
using System.Text;

namespace PuroSuco.Core;

public sealed class CGenerator
{
    private readonly StringBuilder _sb = new();
    private int _indent;
    private bool _inMain;

    public string Generate(CompilationUnit unit)
    {
        _sb.Clear();
        _indent = 0;
        _inMain = false;

        // Runtime headers and utility helpers
        _sb.AppendLine("// 🥤 PuroSuco Compiler - C Backend (C99/C11)");
        _sb.AppendLine("#define _CRT_SECURE_NO_WARNINGS");
        _sb.AppendLine("#include <stdio.h>");
        _sb.AppendLine("#include <stdlib.h>");
        _sb.AppendLine("#include <stdbool.h>");
        _sb.AppendLine("#include <string.h>");
        _sb.AppendLine();
        _sb.AppendLine("/* --- Runtime PuroSuco --- */");
        _sb.AppendLine("static inline char* _ps_str_dup(const char* s) {");
        _sb.AppendLine("    if (!s) return NULL;");
        _sb.AppendLine("    size_t len = strlen(s);");
        _sb.AppendLine("    char* copy = (char*)malloc(len + 1);");
        _sb.AppendLine("    if (copy) strcpy(copy, s);");
        _sb.AppendLine("    return copy;");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline char* _ps_int_to_str(long long val) {");
        _sb.AppendLine("    char buf[64];");
        _sb.AppendLine("    snprintf(buf, sizeof(buf), \"%lld\", val);");
        _sb.AppendLine("    return _ps_str_dup(buf);");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline char* _ps_double_to_str(double val) {");
        _sb.AppendLine("    char buf[64];");
        _sb.AppendLine("    snprintf(buf, sizeof(buf), \"%g\", val);");
        _sb.AppendLine("    return _ps_str_dup(buf);");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline char* _ps_bool_to_str(bool val) {");
        _sb.AppendLine("    return _ps_str_dup(val ? \"true\" : \"false\");");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline char* _ps_str_concat(const char* s1, const char* s2) {");
        _sb.AppendLine("    if (!s1) s1 = \"\";");
        _sb.AppendLine("    if (!s2) s2 = \"\";");
        _sb.AppendLine("    size_t len1 = strlen(s1);");
        _sb.AppendLine("    size_t len2 = strlen(s2);");
        _sb.AppendLine("    char* res = (char*)malloc(len1 + len2 + 1);");
        _sb.AppendLine("    if (res) {");
        _sb.AppendLine("        memcpy(res, s1, len1);");
        _sb.AppendLine("        memcpy(res + len1, s2, len2);");
        _sb.AppendLine("        res[len1 + len2] = '\\0';");
        _sb.AppendLine("    }");
        _sb.AppendLine("    return res;");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline bool _ps_str_eq(const char* a, const char* b) {");
        _sb.AppendLine("    if (a == b) return true;");
        _sb.AppendLine("    if (!a || !b) return false;");
        _sb.AppendLine("    return strcmp(a, b) == 0;");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("static inline char* _ps_read_line(void) {");
        _sb.AppendLine("    char buf[1024];");
        _sb.AppendLine("    if (fgets(buf, sizeof(buf), stdin)) {");
        _sb.AppendLine("        size_t len = strlen(buf);");
        _sb.AppendLine("        while (len > 0 && (buf[len - 1] == '\\n' || buf[len - 1] == '\\r')) {");
        _sb.AppendLine("            buf[len - 1] = '\\0';");
        _sb.AppendLine("            len--;");
        _sb.AppendLine("        }");
        _sb.AppendLine("        return _ps_str_dup(buf);");
        _sb.AppendLine("    }");
        _sb.AppendLine("    return _ps_str_dup(\"\");");
        _sb.AppendLine("}");
        _sb.AppendLine();
        _sb.AppendLine("#define _PS_TO_STR(x) _Generic((x), \\");
        _sb.AppendLine("    bool: _ps_bool_to_str, \\");
        _sb.AppendLine("    char: _ps_int_to_str, \\");
        _sb.AppendLine("    int: _ps_int_to_str, \\");
        _sb.AppendLine("    long: _ps_int_to_str, \\");
        _sb.AppendLine("    long long: _ps_int_to_str, \\");
        _sb.AppendLine("    float: _ps_double_to_str, \\");
        _sb.AppendLine("    double: _ps_double_to_str, \\");
        _sb.AppendLine("    char*: _ps_str_dup, \\");
        _sb.AppendLine("    const char*: _ps_str_dup, \\");
        _sb.AppendLine("    default: _ps_str_dup \\");
        _sb.AppendLine(")(x)");
        _sb.AppendLine();
        _sb.AppendLine("#define MANDA_AI(x) _Generic((x), \\");
        _sb.AppendLine("    bool: printf(\"%s\\n\", (x) ? \"true\" : \"false\"), \\");
        _sb.AppendLine("    char: printf(\"%c\\n\", (char)(x)), \\");
        _sb.AppendLine("    int: printf(\"%d\\n\", (int)(x)), \\");
        _sb.AppendLine("    long: printf(\"%ld\\n\", (long)(x)), \\");
        _sb.AppendLine("    long long: printf(\"%lld\\n\", (long long)(x)), \\");
        _sb.AppendLine("    float: printf(\"%g\\n\", (double)(x)), \\");
        _sb.AppendLine("    double: printf(\"%g\\n\", (double)(x)), \\");
        _sb.AppendLine("    char*: printf(\"%s\\n\", (const char*)(x)), \\");
        _sb.AppendLine("    const char*: printf(\"%s\\n\", (const char*)(x)), \\");
        _sb.AppendLine("    default: printf(\"%p\\n\", (void*)(x)) \\");
        _sb.AppendLine(")");
        _sb.AppendLine();
        _sb.AppendLine("#define SOLTA_AI(x) _Generic((x), \\");
        _sb.AppendLine("    bool: printf(\"%s\", (x) ? \"true\" : \"false\"), \\");
        _sb.AppendLine("    char: printf(\"%c\", (char)(x)), \\");
        _sb.AppendLine("    int: printf(\"%d\", (int)(x)), \\");
        _sb.AppendLine("    long: printf(\"%ld\", (long)(x)), \\");
        _sb.AppendLine("    long long: printf(\"%lld\", (long long)(x)), \\");
        _sb.AppendLine("    float: printf(\"%g\", (double)(x)), \\");
        _sb.AppendLine("    double: printf(\"%g\", (double)(x)), \\");
        _sb.AppendLine("    char*: printf(\"%s\", (const char*)(x)), \\");
        _sb.AppendLine("    const char*: printf(\"%s\", (const char*)(x)), \\");
        _sb.AppendLine("    default: printf(\"%p\", (void*)(x)) \\");
        _sb.AppendLine(")");
        _sb.AppendLine();
        _sb.AppendLine("#define FALA_TU() _ps_read_line()");
        _sb.AppendLine("/* ------------------------- */");
        _sb.AppendLine();

        // Collect global statements vs declarations
        var globalStatements = new List<StatementSyntax>();
        var otherMembers = new List<MemberSyntax>();

        foreach (var member in unit.Members)
        {
            if (member is GlobalStatementSyntax g)
                globalStatements.Add(g.Statement);
            else
                otherMembers.Add(member);
        }

        // Emit forward declarations of functions
        EmitPrototypes(otherMembers);

        // Emit other members (functions, classes/modules)
        foreach (var member in otherMembers)
            WriteMember(member);

        // If there are top-level global statements, wrap in int main()
        if (globalStatements.Count > 0)
        {
            _sb.AppendLine("int main(int argc, char** argv)");
            WriteOpenBrace();
            _inMain = true;
            foreach (var stmt in globalStatements)
                WriteStatement(stmt);
            WriteIndent();
            _sb.AppendLine("return 0;");
            _inMain = false;
            WriteCloseBrace();
        }

        return _sb.ToString();
    }

    private void EmitPrototypes(IReadOnlyList<MemberSyntax> members)
    {
        foreach (var member in members)
        {
            switch (member)
            {
                case FunctionDeclarationSyntax f:
                    EmitFunctionPrototype(f);
                    break;

                case ClassDeclarationSyntax c:
                    EmitPrototypes(c.Members);
                    break;

                case TypeDeclarationSyntax t:
                    EmitPrototypes(t.Members);
                    break;

                case NamespaceDeclarationSyntax ns:
                    EmitPrototypes(ns.Members);
                    break;
            }
        }
    }

    private void EmitFunctionPrototype(FunctionDeclarationSyntax f)
    {
        var isMain = string.Equals(f.Name, "Main", StringComparison.OrdinalIgnoreCase);
        if (isMain) return; // main prototype is standard

        var retType = MapType(f.ReturnType);
        _sb.Append(retType).Append(' ').Append(f.Name).Append('(');
        if (f.Parameters.Count == 0)
        {
            _sb.Append("void");
        }
        else
        {
            _sb.Append(string.Join(", ", f.Parameters.Select(p => $"{MapType(p.TypeName)} {p.Name}")));
        }
        _sb.AppendLine(");");
    }

    private void WriteMember(MemberSyntax member)
    {
        switch (member)
        {
            case UsingDirectiveSyntax:
                // C uses headers, ignored or mapped
                break;

            case NamespaceDeclarationSyntax ns:
                foreach (var child in ns.Members)
                    WriteMember(child);
                break;

            case GlobalStatementSyntax g:
                WriteStatement(g.Statement);
                break;

            case ClassDeclarationSyntax c:
                foreach (var child in c.Members)
                    WriteMember(child);
                break;

            case TypeDeclarationSyntax t:
                var kind = Keywords.Normalize(t.TypeKindKeyword);
                if (kind is "MINI_TROPA" or "TROPA")
                {
                    // If it contains fields, we can emit a struct
                    var fields = t.Members.OfType<GlobalStatementSyntax>()
                        .Select(g => g.Statement)
                        .OfType<VariableDeclarationSyntax>()
                        .ToList();

                    if (fields.Count > 0)
                    {
                        WriteIndent();
                        _sb.Append("typedef struct ").Append(t.Name).AppendLine(" {");
                        _indent++;
                        foreach (var f in fields)
                        {
                            WriteIndent();
                            _sb.Append(MapType(f.TypeName)).Append(' ').Append(f.Identifier).AppendLine(";");
                        }
                        _indent--;
                        WriteIndent();
                        _sb.Append("} ").Append(t.Name).AppendLine(";");
                        _sb.AppendLine();
                    }

                    // Emit member functions
                    foreach (var child in t.Members.Where(m => m is not GlobalStatementSyntax))
                        WriteMember(child);
                }
                else
                {
                    foreach (var child in t.Members)
                        WriteMember(child);
                }
                break;

            case FunctionDeclarationSyntax f:
                var isMain = string.Equals(f.Name, "Main", StringComparison.OrdinalIgnoreCase);
                WriteIndent();
                if (isMain)
                {
                    _inMain = true;
                    _sb.AppendLine("int main(int argc, char** argv)");
                }
                else
                {
                    _sb.Append(MapType(f.ReturnType)).Append(' ').Append(f.Name).Append('(');
                    if (f.Parameters.Count == 0)
                    {
                        _sb.Append("void");
                    }
                    else
                    {
                        _sb.Append(string.Join(", ", f.Parameters.Select(p => $"{MapType(p.TypeName)} {p.Name}")));
                    }
                    _sb.AppendLine(")");
                }

                WriteBlock(f.Body, isMain);
                _inMain = false;
                _sb.AppendLine();
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
                {
                    _sb.Append(" = ").Append(WriteExpression(v.Initializer));
                }
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

            case ForeachStatementSyntax fe:
                // C doesn't have native foreach; we emit an error comment or basic loop
                WriteIndent();
                _sb.Append("// foreach not natively supported in C backend: ").Append(fe.Identifier).AppendLine();
                break;

            case TryStatementSyntax tryStmt:
                // C doesn't have try/catch; we execute the try block directly
                WriteIndent();
                _sb.AppendLine("// try block");
                WriteBlock(tryStmt.TryBlock);
                break;

            case ThrowStatementSyntax th:
                WriteIndent();
                _sb.Append("fprintf(stderr, \"Erro/Throw: \"); ");
                if (th.Expression is not null)
                    _sb.Append("MANDA_AI(").Append(WriteExpression(th.Expression)).Append("); ");
                _sb.AppendLine("exit(1);");
                break;

            case ReturnStatementSyntax r:
                WriteIndent();
                _sb.Append("return");
                if (r.Expression is not null)
                {
                    _sb.Append(' ').Append(WriteExpression(r.Expression));
                }
                else if (_inMain)
                {
                    _sb.Append(" 0");
                }
                _sb.AppendLine(";");
                break;

            case BreakStatementSyntax:
                WriteIndent();
                _sb.AppendLine("break;");
                break;

            case ContinueStatementSyntax:
                WriteIndent();
                _sb.AppendLine("continue;");
                break;
        }
    }

    private void WriteBlock(BlockStatementSyntax block, bool isMain = false)
    {
        WriteOpenBrace();
        foreach (var statement in block.Statements)
            WriteStatement(statement);

        if (isMain && !EndsWithReturn(block))
        {
            WriteIndent();
            _sb.AppendLine("return 0;");
        }
        WriteCloseBrace();
    }

    private static bool EndsWithReturn(BlockStatementSyntax block)
    {
        return block.Statements.Count > 0 && block.Statements[^1] is ReturnStatementSyntax;
    }

    private string WriteExpression(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax l => WriteLiteral(l),
        NameExpressionSyntax n => n.Identifier,
        BinaryExpressionSyntax b => WriteBinaryExpression(b),
        CallExpressionSyntax c => WriteCallExpression(c),
        MemberAccessExpressionSyntax m => $"{WriteExpression(m.Target)}.{m.MemberName}",
        NewExpressionSyntax nw => $"/* new */ ({MapType(nw.TypeName)}){{ {string.Join(", ", nw.Arguments.Select(WriteExpression))} }}",
        _ => throw new NotSupportedException(expression.GetType().Name)
    };

    private string WriteBinaryExpression(BinaryExpressionSyntax b)
    {
        var op = b.Operator;
        var leftStr = IsStringExpression(b.Left);
        var rightStr = IsStringExpression(b.Right);

        if (op == "+" && (leftStr || rightStr))
        {
            return $"_ps_str_concat(_PS_TO_STR({WriteExpression(b.Left)}), _PS_TO_STR({WriteExpression(b.Right)}))";
        }

        if ((op == "==" || op == "!=") && (leftStr || rightStr))
        {
            var eq = $"_ps_str_eq(_PS_TO_STR({WriteExpression(b.Left)}), _PS_TO_STR({WriteExpression(b.Right)}))";
            return op == "==" ? eq : $"(!{eq})";
        }

        return $"{WriteExpression(b.Left)} {b.Operator} {WriteExpression(b.Right)}";
    }

    private static bool IsStringExpression(ExpressionSyntax expr) => expr switch
    {
        LiteralExpressionSyntax l => Keywords.Normalize(l.TypeName) == "PAPO",
        CallExpressionSyntax c => Keywords.Normalize(c.Name) == "FALA_TU",
        BinaryExpressionSyntax b => b.Operator == "+" && (IsStringExpression(b.Left) || IsStringExpression(b.Right)),
        _ => false
    };

    private string WriteCallExpression(CallExpressionSyntax c)
    {
        var normalized = Keywords.Normalize(c.Name);
        return normalized switch
        {
            "MANDA_AI" => $"MANDA_AI({string.Join(", ", c.Arguments.Select(WriteExpression))})",
            "SOLTA_AI" => $"SOLTA_AI({string.Join(", ", c.Arguments.Select(WriteExpression))})",
            "FALA_TU" => "_ps_read_line()",
            _ => $"{c.Name}({string.Join(", ", c.Arguments.Select(WriteExpression))})"
        };
    }

    private static string WriteLiteral(LiteralExpressionSyntax literal) => Keywords.Normalize(literal.TypeName) switch
    {
        "PAPO" => $"\"{literal.Value?.ToString()?.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
        "CONFERE" => literal.Value is true ? "true" : "false",
        "TEM_NADA_AI" => "NULL",
        "NUMERO_QUEBRADO" => Convert.ToString(literal.Value, CultureInfo.InvariantCulture) ?? "0.0",
        _ => literal.Value?.ToString() ?? "0"
    };

    private static string MapType(string type) => Keywords.Normalize(type) switch
    {
        "NUMERO" => "int",
        "NUMERO_QUEBRADO" => "double",
        "NUMERO_BRUTO" => "long long",
        "GRANA" => "double",
        "LETRA" => "char",
        "PAPO" => "char*",
        "CONFERE" => "bool",
        "QUALQUER_COISA" => "void*",
        "SEI_LA" => "auto",
        "VAI_NA_FE" => "void*",
        "VOLTA_NADA" => "void",
        _ => type
    };

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
