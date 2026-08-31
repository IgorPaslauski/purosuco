using System.Text;

namespace PuroSuco.Core;

public static class AstPrinter
{
    public static string Print(SyntaxNode node)
    {
        var sb = new StringBuilder();
        Write(node, sb, "", true);
        return sb.ToString();
    }

    private static void Write(SyntaxNode node, StringBuilder sb, string indent, bool last)
    {
        sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(Label(node));
        indent += last ? "    " : "│   ";

        var children = Children(node).ToArray();
        for (var i = 0; i < children.Length; i++)
            Write(children[i], sb, indent, i == children.Length - 1);
    }

    private static string Label(SyntaxNode node) => node switch
    {
        CompilationUnit => "CompilationUnit",
        UsingDirectiveSyntax u => $"CHAMA {u.NamespaceName}",
        NamespaceDeclarationSyntax ns => $"QUEBRADA {ns.Name}",
        ClassDeclarationSyntax c => $"TROPA {c.Name}",
        TypeDeclarationSyntax t => $"{t.TypeKindKeyword} {t.Name}",
        FunctionDeclarationSyntax f => $"Function {f.ReturnType} {f.Name}",
        ParameterSyntax p => $"Parameter {p.TypeName} {p.Name}",
        GlobalStatementSyntax => "GlobalStatement",
        VariableDeclarationSyntax v => $"Variable {v.TypeName} {v.Identifier}",
        AssignmentStatementSyntax a => $"Assignment {a.Identifier}",
        LiteralExpressionSyntax l => $"Literal {l.Value ?? "null"}",
        NameExpressionSyntax n => $"Name {n.Identifier}",
        BinaryExpressionSyntax b => $"Binary {b.Operator}",
        CallExpressionSyntax c => $"Call {c.Name}",
        MemberAccessExpressionSyntax m => $"MemberAccess {m.MemberName}",
        AwaitExpressionSyntax => "Await (PERAI)",
        NewExpressionSyntax nw => $"New (BROTOU) {nw.TypeName}",
        IfStatementSyntax => "If (TA_CERTO_ISSO)",
        WhileStatementSyntax => "While (ENQUANTO_TANKAR)",
        DoWhileStatementSyntax => "DoWhile (FAZ_PRIMEIRO)",
        ForStatementSyntax => "For (BORA_BILL)",
        ForeachStatementSyntax fe => $"Foreach (PRA_CADA_UM {fe.Identifier})",
        TryStatementSyntax => "Try (VAI_DAR_BOM)",
        CatchClauseSyntax cc => $"Catch (DEU_RUIM {cc.ExceptionType ?? ""})",
        ThrowStatementSyntax => "Throw (AI_TU_ME_QUEBRA)",
        ReturnStatementSyntax => "Return (TOMA)",
        BlockStatementSyntax => "Block",
        BreakStatementSyntax => "Break (CHEGA)",
        ContinueStatementSyntax => "Continue (SEGUE_O_JOGO)",
        ExpressionStatementSyntax => "ExpressionStatement",
        _ => node.GetType().Name
    };

    private static IEnumerable<SyntaxNode> Children(SyntaxNode node)
    {
        switch (node)
        {
            case CompilationUnit c:
                foreach (var x in c.Members) yield return x;
                break;
            case NamespaceDeclarationSyntax ns:
                foreach (var x in ns.Members) yield return x;
                break;
            case ClassDeclarationSyntax c:
                foreach (var x in c.Members) yield return x;
                break;
            case TypeDeclarationSyntax t:
                foreach (var x in t.Members) yield return x;
                break;
            case FunctionDeclarationSyntax f:
                foreach (var p in f.Parameters) yield return p;
                yield return f.Body;
                break;
            case GlobalStatementSyntax g:
                yield return g.Statement;
                break;
            case BlockStatementSyntax b:
                foreach (var x in b.Statements) yield return x;
                break;
            case VariableDeclarationSyntax v when v.Initializer is not null:
                yield return v.Initializer;
                break;
            case AssignmentStatementSyntax a:
                yield return a.Expression;
                break;
            case ExpressionStatementSyntax e:
                yield return e.Expression;
                break;
            case IfStatementSyntax i:
                yield return i.Condition;
                yield return i.Then;
                if (i.Else is not null) yield return i.Else;
                break;
            case WhileStatementSyntax w:
                yield return w.Condition;
                yield return w.Body;
                break;
            case DoWhileStatementSyntax dw:
                yield return dw.Body;
                yield return dw.Condition;
                break;
            case ForStatementSyntax forStmt:
                if (forStmt.Initializer is not null) yield return forStmt.Initializer;
                if (forStmt.Condition is not null) yield return forStmt.Condition;
                if (forStmt.Increment is not null) yield return forStmt.Increment;
                yield return forStmt.Body;
                break;
            case ForeachStatementSyntax fe:
                yield return fe.Collection;
                yield return fe.Body;
                break;
            case TryStatementSyntax tryStmt:
                yield return tryStmt.TryBlock;
                foreach (var c in tryStmt.CatchClauses) yield return c;
                if (tryStmt.FinallyBlock is not null) yield return tryStmt.FinallyBlock;
                break;
            case CatchClauseSyntax cc:
                yield return cc.Body;
                break;
            case ThrowStatementSyntax th when th.Expression is not null:
                yield return th.Expression;
                break;
            case ReturnStatementSyntax r when r.Expression is not null:
                yield return r.Expression;
                break;
            case BinaryExpressionSyntax b:
                yield return b.Left;
                yield return b.Right;
                break;
            case CallExpressionSyntax c:
                foreach (var a in c.Arguments) yield return a;
                break;
            case MemberAccessExpressionSyntax m:
                yield return m.Target;
                break;
            case AwaitExpressionSyntax a:
                yield return a.Expression;
                break;
            case NewExpressionSyntax nw:
                foreach (var a in nw.Arguments) yield return a;
                break;
        }
    }
}
