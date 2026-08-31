namespace PuroSuco.Core;

public sealed class Transpiler
{
    public string ToCSharp(string source)
    {
        var tree = new Parser(source).ParseCompilationUnit();
        return new CSharpGenerator().Generate(tree);
    }
}
