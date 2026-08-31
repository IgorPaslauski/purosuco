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
        ClassDeclarationSyntax c => $"TROPA {c.Name}",
        FunctionDeclarationSyntax f => $"Function {f.ReturnType} {f.Name}",
        ParameterSyntax p => $"Parameter {p.TypeName} {p.Name}",
        GlobalStatementSyntax => "GlobalStatement",
        VariableDeclarationSyntax v => $"Variable {v.TypeName} {v.Identifier}",
        AssignmentStatementSyntax a => $"Assignment {a.Identifier}",
        LiteralExpressionSyntax l => $"Literal {l.Value ?? "null"}",
        NameExpressionSyntax n => $"Name {n.Identifier}",
        BinaryExpressionSyntax b => $"Binary {b.Operator}",
        CallExpressionSyntax c => $"Call {c.Name}",
        IfStatementSyntax => "If",
        WhileStatementSyntax => "While",
        ForStatementSyntax => "For (BORA_BILL)",
        ReturnStatementSyntax => "Return",
        BlockStatementSyntax => "Block",
        BreakStatementSyntax => "Break",
        ContinueStatementSyntax => "Continue",
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
            case ClassDeclarationSyntax c:
                foreach (var x in c.Members) yield return x;
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
            case ForStatementSyntax forStmt:
                if (forStmt.Initializer is not null) yield return forStmt.Initializer;
                if (forStmt.Condition is not null) yield return forStmt.Condition;
                if (forStmt.Increment is not null) yield return forStmt.Increment;
                yield return forStmt.Body;
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
        }
    }
}
