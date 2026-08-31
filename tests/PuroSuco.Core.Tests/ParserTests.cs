using PuroSuco.Core;
using Xunit;

namespace PuroSuco.Core.Tests;

public sealed class ParserTests
{
    [Fact]
    public void Parses_variable_assignment_and_if()
    {
        const string source = """
NUMERO idade RECEBA 23;

TA_CERTO_ISSO idade >= 18 {
    MANDA_AI("RECEBA!");
}
""";

        var tree = new Parser(source).ParseCompilationUnit();

        Assert.Equal(2, tree.Members.Count);
        Assert.IsType<VariableDeclarationSyntax>(((GlobalStatementSyntax)tree.Members[0]).Statement);
        Assert.IsType<IfStatementSyntax>(((GlobalStatementSyntax)tree.Members[1]).Statement);
    }

    [Fact]
    public void Detects_string_assigned_to_number()
    {
        var tree = new Parser("NUMERO idade RECEBA \"vinte\";").ParseCompilationUnit();
        var diagnostics = new SemanticAnalyzer().Analyze(tree).Diagnostics;

        Assert.Contains(diagnostics, d => d.Code == "PS003");
    }

    [Fact]
    public void Detects_division_by_zero()
    {
        var tree = new Parser("NUMERO x RECEBA 10 / 0;").ParseCompilationUnit();
        var diagnostics = new SemanticAnalyzer().Analyze(tree).Diagnostics;

        Assert.Contains(diagnostics, d => d.Code == "PS021");
    }
}
