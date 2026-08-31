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

    [Fact]
    public void Normalizes_accents_and_case_in_keywords()
    {
        const string source = """
AMOSTRADINHO TROPA Conta {
    AMOSTRADINHO SEMPRE_FOI_ASSIM CONFERE TemSaldo(NÚMERO saldo) {
        TÁ_CERTO_ISSO (saldo > 0) {
            TOMA CONFIA;
        } NÃO_TÁ_NÃO {
            TOMA CONFIA_NÃO;
        }
    }
}
""";
        var csharp = new Transpiler().ToCSharp(source);
        Assert.Contains("class Conta", csharp);
        Assert.Contains("public static bool TemSaldo(int saldo)", csharp);
        Assert.Contains("if (saldo > 0)", csharp);
        Assert.Contains("return true;", csharp);
        Assert.Contains("else", csharp);
        Assert.Contains("return false;", csharp);
    }

    [Fact]
    public void Transpiles_try_catch_finally_and_throw()
    {
        const string source = """
CHAMA System;

AMOSTRADINHO TROPA Servico {
    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Executar(NUMERO valor) {
        VAI_DAR_BOM {
            TA_CERTO_ISSO (valor < 0) {
                AI_TU_ME_QUEBRA BROTOU Exception("Valor negativo!");
            }
            MANDA_AI("Tudo certo!");
        } DEU_RUIM (Exception ex) {
            MANDA_AI("Erro capturado");
        } DE_QUALQUER_JEITO {
            MANDA_AI("Fim do processo");
        }
    }
}
""";
        var csharp = new Transpiler().ToCSharp(source);
        Assert.Contains("using System;", csharp);
        Assert.Contains("try", csharp);
        Assert.Contains("throw new Exception(\"Valor negativo!\");", csharp);
        Assert.Contains("catch (Exception ex)", csharp);
        Assert.Contains("finally", csharp);
    }

    [Fact]
    public void Transpiles_foreach_and_do_while()
    {
        const string source = """
AMOSTRADINHO TROPA Colecoes {
    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Iterar(SEI_LA lista) {
        PRA_CADA_UM (PAPO item DENTRO_DE lista) {
            MANDA_AI(item);
        }

        NUMERO k RECEBA 0;
        FAZ_PRIMEIRO {
            k RECEBA k + 1;
        } ENQUANTO_TANKAR (k < 3);
    }
}
""";
        var csharp = new Transpiler().ToCSharp(source);
        Assert.Contains("foreach (string item in lista)", csharp);
        Assert.Contains("do", csharp);
        Assert.Contains("while (k < 3);", csharp);
    }

    [Fact]
    public void Ignores_single_and_multi_line_comments()
    {
        const string source = """
// Comentario de linha
/* Comentario
   em bloco */
NUMERO x RECEBA 42; // Outro comentario
""";
        var tree = new Parser(source).ParseCompilationUnit();
        Assert.Single(tree.Members);
    }
}
