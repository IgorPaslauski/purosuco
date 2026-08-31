using PuroSuco.Core;
using Xunit;

namespace PuroSuco.Core.Tests;

public sealed class CGeneratorTests
{
    [Fact]
    public void Transpiles_hello_suco_to_valid_C()
    {
        const string source = """
AMOSTRADINHO TROPA Programa {
    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Main() {
        NUMERO idade RECEBA 17;
        TA_CERTO_ISSO idade >= 18 {
            MANDA_AI("RECEBA!");
        }
        NAO_TA_NAO {
            MANDA_AI("Ai nao.");
        }
    }
}
""";

        var cCode = new Transpiler().ToC(source);

        Assert.Contains("#include <stdio.h>", cCode);
        Assert.Contains("int main(int argc, char** argv)", cCode);
        Assert.Contains("int idade = 17;", cCode);
        Assert.Contains("if (idade >= 18)", cCode);
        Assert.Contains("MANDA_AI(\"RECEBA!\");", cCode);
        Assert.Contains("MANDA_AI(\"Ai nao.\");", cCode);
        Assert.Contains("return 0;", cCode);
    }

    [Fact]
    public void Transpiles_functions_and_loops_to_C()
    {
        const string source = """
AMOSTRADINHO TROPA Algoritmos {
    AMOSTRADINHO SEMPRE_FOI_ASSIM NUMERO Somar(NUMERO a, NUMERO b) {
        TOMA a + b;
    }

    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Main() {
        NUMERO total RECEBA Somar(10, 20);
        BORA_BILL (NUMERO i RECEBA 0; i < 5; i RECEBA i + 1) {
            MANDA_AI(i);
        }
    }
}
""";

        var cCode = new Transpiler().ToC(source);

        Assert.Contains("int Somar(int a, int b);", cCode); // prototype
        Assert.Contains("int Somar(int a, int b)", cCode);   // definition
        Assert.Contains("return a + b;", cCode);
        Assert.Contains("for (int i = 0; i < 5; i = i + 1)", cCode);
        Assert.Contains("int total = Somar(10, 20);", cCode);
    }

    [Fact]
    public void Transpiles_string_concatenation_in_C()
    {
        const string source = """
AMOSTRADINHO TROPA Strings {
    AMOSTRADINHO SEMPRE_FOI_ASSIM VOLTA_NADA Main() {
        PAPO nome RECEBA "Mundo";
        MANDA_AI("Ola, " + nome + "!");
    }
}
""";

        var cCode = new Transpiler().ToC(source);

        Assert.Contains("char* nome = \"Mundo\";", cCode);
        Assert.Contains("_ps_str_concat", cCode);
        Assert.Contains("_PS_TO_STR", cCode);
    }

    [Fact]
    public void Transpiles_global_statements_to_C_main()
    {
        const string source = """
NUMERO x RECEBA 42;
MANDA_AI(x);
""";

        var cCode = new Transpiler().ToC(source);

        Assert.Contains("int main(int argc, char** argv)", cCode);
        Assert.Contains("int x = 42;", cCode);
        Assert.Contains("MANDA_AI(x);", cCode);
        Assert.Contains("return 0;", cCode);
    }
}
