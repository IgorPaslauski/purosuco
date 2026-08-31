namespace PuroSuco.Core;

public sealed class Transpiler
{
    public string ToCSharp(string source)
    {
        var tree = new Parser(source).ParseCompilationUnit();
        return new CSharpGenerator().Generate(tree);
    }

    public string ToC(string source)
    {
        var tree = new Parser(source).ParseCompilationUnit();
        return new CGenerator().Generate(tree);
    }
}
