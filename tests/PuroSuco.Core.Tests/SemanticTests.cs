using PuroSuco.Core;
using Xunit;

namespace PuroSuco.Core.Tests;

public sealed class SemanticTests
{
    [Fact]
    public void Parses_class_and_function()
    {
        const string source = """
AMOSTRADINHO TROPA Programa {
    AMOSTRADINHO SEMPRE_FOI_ASSIM NUMERO Soma(NUMERO a, NUMERO b) {
        TOMA a + b;
    }
}
""";

        var tree = new Parser(source).ParseCompilationUnit();

        var cls = Assert.IsType<ClassDeclarationSyntax>(tree.Members.Single());
        var fn = Assert.IsType<FunctionDeclarationSyntax>(cls.Members.Single());

        Assert.Equal("Soma", fn.Name);
        Assert.Equal(2, fn.Parameters.Count);
    }

    [Fact]
    public void Resolves_parameters_inside_function()
    {
        const string source = """
NUMERO Soma(NUMERO a, NUMERO b) {
    TOMA a + b;
}
""";

        var tree = new Parser(source).ParseCompilationUnit();
        var model = new SemanticAnalyzer().Analyze(tree);

        Assert.DoesNotContain(model.Diagnostics, d => d.Code == "PS017");
    }

    [Fact]
    public void Detects_wrong_return_type()
    {
        const string source = """
NUMERO Nome() {
    TOMA "Igor";
}
""";

        var model = new SemanticAnalyzer().Analyze(new Parser(source).ParseCompilationUnit());

        Assert.Contains(model.Diagnostics, d => d.Code == "PS051");
    }
}
