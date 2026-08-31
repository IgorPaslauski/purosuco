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

    [Fact]
    public void Transpiles_while_and_for_and_input()
    {
        const string source = """
AMOSTRADINHO TROPA Loops {
    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Teste() {
        NUMERO i RECEBA 0;
        ENQUANTO_TANKAR i < 5 {
            MANDA_AI(i);
            i RECEBA i + 1;
        }

        BORA_BILL (NUMERO j RECEBA 0; j < 3; j RECEBA j + 1) {
            MANDA_AI(j);
        }

        PAPO entrada RECEBA FALA_TU();
    }
}
""";
        var csharp = new Transpiler().ToCSharp(source);
        Assert.Contains("while (i < 5)", csharp);
        Assert.Contains("for (int j = 0; j < 3; j = j + 1)", csharp);
        Assert.Contains("Console.ReadLine()", csharp);
    }
}
