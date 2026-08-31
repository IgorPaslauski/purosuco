using PuroSuco.Core;
using System.Diagnostics;

if (args.Length < 2)
{
    ShowHelp();
    return;
}

var command = args[0].ToLowerInvariant();
var file = Path.GetFullPath(args[1]);

if (!File.Exists(file))
{
    Console.Error.WriteLine($"PS404 — CADÊ ESSE CARA?\nArquivo não encontrado: {file}");
    Environment.ExitCode = 1;
    return;
}

var source = await File.ReadAllTextAsync(file);

try
{
    switch (command)
    {
        case "traduz":
        case "transpile":
            await Transpile(file, source);
            break;

        case "roda":
        case "run":
            await Run(file, source);
            break;

        case "tokens":
            foreach (var token in new Lexer(source).Lex())
                Console.WriteLine($"{token.Kind,-12} {token.Text}");
            break;

        case "ast":
            var tree = new Parser(source).ParseCompilationUnit();
            Console.WriteLine(AstPrinter.Print(tree));
            break;

        case "check":
            var parsed = new Parser(source).ParseCompilationUnit();
            var model = new SemanticAnalyzer().Analyze(parsed);
            var diagnostics = model.Diagnostics;
            if (diagnostics.Count == 0)
            {
                Console.WriteLine("Tá certo isso? Tá.");
            }
            else
            {
                foreach (var diagnostic in diagnostics)
                    Console.WriteLine($"{diagnostic.Code} — {diagnostic.Title}\n{diagnostic.Message}\n");
                Environment.ExitCode = diagnostics.Any(d => d.IsError) ? 1 : 0;
            }
            break;

        case "formata":
        case "format":
            var formatted = new Formatter().Format(source);
            await File.WriteAllTextAsync(file, formatted);
            Console.WriteLine("FORMATAR ESSA BAGUNÇA: RECEBA.");
            break;

        default:
            ShowHelp();
            break;
    }
}
catch (PuroSucoException ex)
{
    Console.Error.WriteLine(ex.ToString());
    Environment.ExitCode = 1;
}

static async Task<string> Transpile(string file, string source)
{
    var csharp = new Transpiler().ToCSharp(source);
    var outFile = Path.ChangeExtension(file, ".g.cs");
    await File.WriteAllTextAsync(outFile, csharp);

    Console.WriteLine("🥤 PuroSuco Compiler");
    Console.WriteLine("Analisando a resenha...");
    Console.WriteLine("Tá certo isso? Tá.");
    Console.WriteLine($"RECEBA: {outFile}");
    return outFile;
}

static async Task Run(string file, string source)
{
    var generated = await Transpile(file, source);
    var tempDir = Path.Combine(Path.GetTempPath(), "purosuco", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempDir);

    try
    {
        var programCs = Path.Combine(tempDir, "Program.cs");
        var csproj = Path.Combine(tempDir, "PuroSuco.Generated.csproj");
        File.Copy(generated, programCs);

        await File.WriteAllTextAsync(csproj, """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
""");

        Console.WriteLine();
        Console.WriteLine("RODANDO A RESENHA...");
        Console.WriteLine();

        var psi = new ProcessStartInfo("dotnet", $"run --project \"{csproj}\"")
        {
            UseShellExecute = false
        };
        using var process = Process.Start(psi);
        if (process is null)
            throw new InvalidOperationException("Não consegui iniciar o dotnet.");

        await process.WaitForExitAsync();
        Environment.ExitCode = process.ExitCode;
    }
    finally
    {
        try { Directory.Delete(tempDir, true); } catch { }
    }
}

static void ShowHelp()
{
    Console.WriteLine("""
PuroSuco CLI

Uso:
  purosuco traduz arquivo.suco
  purosuco roda arquivo.suco
  purosuco tokens arquivo.suco
  purosuco ast arquivo.suco
  purosuco check arquivo.suco
  purosuco formata arquivo.suco

PuroSuco — a linguagem que compila o Brasil.
""");
}
