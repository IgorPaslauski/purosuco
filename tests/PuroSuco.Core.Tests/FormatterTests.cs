using PuroSuco.Core;
using Xunit;

namespace PuroSuco.Core.Tests;

public sealed class FormatterTests
{
    [Fact]
    public void Formats_simple_program()
    {
        const string source = "NUMERO idade RECEBA 23; TA_CERTO_ISSO idade >= 18 { MANDA_AI(\"RECEBA!\"); }";

        var formatted = new Formatter().Format(source);

        Assert.Contains("NUMERO idade RECEBA 23;", formatted);
        Assert.Contains("TA_CERTO_ISSO idade >= 18 {", formatted);
    }

    [Fact]
    public void Creates_numeric_string_quick_fix()
    {
        const string source = "NUMERO idade RECEBA \"23\";";
        var model = new SemanticAnalyzer().Analyze(new Parser(source).ParseCompilationUnit());
        var diagnostic = Assert.Single(model.Diagnostics.Where(d => d.Code == "PS003"));

        var fixes = CodeFixProvider.GetFixes(source, diagnostic);

        Assert.Contains(fixes, f => f.Replacement == "23");
    }
}
