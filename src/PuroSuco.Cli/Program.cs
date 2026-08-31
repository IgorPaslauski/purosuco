using PuroSuco.Core;
using System.Diagnostics;
using System.IO.Compression;

if (args.Length == 0)
{
    ShowHelp();
    return;
}

var command = args[0].ToLowerInvariant();

if (command is "instala" or "install")
{
    await InstallToPath();
    return;
}

if (command is "ajuda" or "help" or "--help" or "-h")
{
    ShowHelp();
    return;
}

if (args.Length < 2)
{
    ShowHelp();
    return;
}

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
        case "roda":
        case "run":
            await RunNative(file, source);
            break;

        case "compila":
        case "build":
        case "compila-c":
        case "build-c":
            await CompileNative(file, source);
            break;

        case "c":
        case "traduz-c":
        case "transpile-c":
        case "traduz":
        case "transpile":
            await TranspileC(file, source);
            break;

        case "cs":
        case "traduz-cs":
        case "transpile-cs":
            await TranspileCSharp(file, source);
            break;

        case "roda-cs":
        case "run-cs":
            await RunDotnet(file, source);
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

static async Task<string> TranspileC(string file, string source)
{
    var cCode = new Transpiler().ToC(source);
    var outFile = Path.ChangeExtension(file, ".c");
    await File.WriteAllTextAsync(outFile, cCode);

    Console.WriteLine("🥤 PuroSuco Compiler");
    Console.WriteLine("Analisando a resenha em C...");
    Console.WriteLine("Tá certo isso? Tá.");
    Console.WriteLine($"RECEBA O C PURO: {outFile}");
    return outFile;
}

static async Task<string> TranspileCSharp(string file, string source)
{
    var csharp = new Transpiler().ToCSharp(source);
    var outFile = Path.ChangeExtension(file, ".g.cs");
    await File.WriteAllTextAsync(outFile, csharp);

    Console.WriteLine("🥤 PuroSuco Compiler (C# Backend)");
    Console.WriteLine("Analisando a resenha...");
    Console.WriteLine("Tá certo isso? Tá.");
    Console.WriteLine($"RECEBA: {outFile}");
    return outFile;
}

static async Task<string?> FindOrDownloadCCompiler()
{
    // 1. Procurar em diretórios conhecidos e PATH
    var candidates = new[] { "gcc", "clang", "tcc", "cl" };
    var appDir = AppContext.BaseDirectory;
    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var purosucoTccDir = Path.Combine(userProfile, ".purosuco", "tcc");

    if (Directory.Exists(purosucoTccDir))
    {
        var found = Directory.GetFiles(purosucoTccDir, "tcc.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (found is not null) return found;
    }

    var localTcc = Path.Combine(appDir, "tcc", "tcc.exe");
    if (File.Exists(localTcc)) return localTcc;

    var wellKnownLocations = new[]
    {
        @"C:\msys64\ucrt64\bin\gcc.exe",
        @"C:\msys64\mingw64\bin\gcc.exe",
        @"C:\msys64\clang64\bin\clang.exe",
        @"C:\ProgramData\chocolatey\bin\gcc.exe",
        @"C:\TDM-GCC-64\bin\gcc.exe",
        @"C:\w64devkit\bin\gcc.exe",
        @"C:\w64devkit\bin\tcc.exe"
    };

    foreach (var loc in wellKnownLocations)
    {
        if (File.Exists(loc)) return loc;
    }

    var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
    var paths = pathEnv.Split(Path.PathSeparator);

    foreach (var compiler in candidates)
    {
        foreach (var p in paths)
        {
            var exeName = OperatingSystem.IsWindows() ? $"{compiler}.exe" : compiler;
            var fullPath = Path.Combine(p.Trim(), exeName);
            if (File.Exists(fullPath)) return fullPath;
        }
    }

    // 2. Se não encontrar no Windows, auto-baixar Tiny C Compiler (TCC) ~2MB
    if (OperatingSystem.IsWindows())
    {
        try
        {
            Console.WriteLine("Nenhum compilador C encontrado no sistema.");
            Console.WriteLine("Baixando TinyCC (compilador C leve e ultrarrápido)...");

            var tccDir = Path.Combine(userProfile, ".purosuco", "tcc");
            Directory.CreateDirectory(tccDir);
            var zipPath = Path.Combine(tccDir, "tcc.zip");

            const string tccUrl = "http://download.savannah.gnu.org/releases/tinycc/tcc-0.9.27-win64-bin.zip";
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var bytes = await http.GetByteArrayAsync(tccUrl);
            await File.WriteAllBytesAsync(zipPath, bytes);

            ZipFile.ExtractToDirectory(zipPath, tccDir, true);
            File.Delete(zipPath);

            var extractedTcc = Directory.GetFiles(tccDir, "tcc.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (extractedTcc is not null)
            {
                Console.WriteLine("TinyCC instalado com sucesso!\n");
                return extractedTcc;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Não foi possível baixar o compilador automático: {ex.Message}");
        }
    }

    return null;
}

static async Task<string?> CompileNative(string file, string source)
{
    var cFile = await TranspileC(file, source);
    var exeFile = Path.ChangeExtension(file, OperatingSystem.IsWindows() ? ".exe" : "");
    var compiler = await FindOrDownloadCCompiler();

    if (compiler is null)
    {
        Console.WriteLine("\n[AVISO] Nenhum compilador C (gcc, clang, tcc, cl) foi encontrado no PATH.");
        Console.WriteLine($"Você pode compilar manualmente com:");
        Console.WriteLine($"  gcc -std=c11 \"{cFile}\" -o \"{exeFile}\"");
        return null;
    }

    var compilerName = Path.GetFileNameWithoutExtension(compiler).ToLowerInvariant();
    var arguments = compilerName switch
    {
        "cl" => $"\"{cFile}\" /Fe:\"{exeFile}\" /O2",
        "tcc" => $"\"{cFile}\" -o \"{exeFile}\"",
        _ => $"-std=c11 \"{cFile}\" -o \"{exeFile}\" -O2"
    };

    var psi = new ProcessStartInfo(compiler, arguments)
    {
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true
    };

    using var proc = Process.Start(psi);
    if (proc is null)
    {
        Console.Error.WriteLine($"Não foi possível executar o compilador: {compiler}.");
        return null;
    }

    var output = await proc.StandardOutput.ReadToEndAsync();
    var error = await proc.StandardError.ReadToEndAsync();
    await proc.WaitForExitAsync();

    if (proc.ExitCode != 0)
    {
        Console.Error.WriteLine($"Erro na compilação ({compilerName}):\n{error}\n{output}");
        Environment.ExitCode = proc.ExitCode;
        return null;
    }

    Console.WriteLine($"SUCESSO! Executável nativo gerado: {exeFile}");
    return exeFile;
}

static async Task RunNative(string file, string source)
{
    var exe = await CompileNative(file, source);
    if (exe is null || !File.Exists(exe))
    {
        Console.Error.WriteLine("Executável não encontrado para execução.");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine();
    Console.WriteLine("RODANDO A RESENHA NATIVA...");
    Console.WriteLine();

    var psi = new ProcessStartInfo(exe)
    {
        UseShellExecute = false
    };
    using var process = Process.Start(psi);
    if (process is not null)
    {
        await process.WaitForExitAsync();
        Environment.ExitCode = process.ExitCode;
    }
}

static async Task RunDotnet(string file, string source)
{
    var generated = await TranspileCSharp(file, source);
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
        Console.WriteLine("RODANDO A RESENHA VIA .NET...");
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

static Task InstallToPath()
{
    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var installDir = Path.Combine(userProfile, ".purosuco", "bin");
    Directory.CreateDirectory(installDir);

    var currentExe = Environment.ProcessPath;
    if (string.IsNullOrEmpty(currentExe) || !File.Exists(currentExe))
    {
        Console.WriteLine($"Crie a pasta de instalação em: {installDir}");
        return Task.CompletedTask;
    }

    var targetExe = Path.Combine(installDir, "purosuco.exe");
    File.Copy(currentExe, targetExe, true);
    Console.WriteLine($"Executável copiado para: {targetExe}");

    // Adicionar ao PATH de Usuário no Windows
    if (OperatingSystem.IsWindows())
    {
        var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
        var paths = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        if (!paths.Any(p => string.Equals(p, installDir, StringComparison.OrdinalIgnoreCase)))
        {
            paths.Add(installDir);
            var newPath = string.Join(";", paths);
            Environment.SetEnvironmentVariable("Path", newPath, EnvironmentVariableTarget.User);
            Console.WriteLine($"PATH atualizado com sucesso: {installDir}");
            Console.WriteLine("Reinicie o terminal para que o comando 'purosuco' fique disponível globalmente!");
        }
        else
        {
            Console.WriteLine($"'{installDir}' já está no seu PATH de usuário.");
        }
    }

    Console.WriteLine("\nRECEBA! Agora você pode usar:");
    Console.WriteLine("  purosuco run caminho/arquivo.suco");
    Console.WriteLine("  purosuco build caminho/arquivo.suco");
    return Task.CompletedTask;
}

static void ShowHelp()
{
    Console.WriteLine("""
🥤 PuroSuco CLI — Compilador Nativo

Uso:
  purosuco run arquivo.suco          # Compila e roda o binário nativo (.exe)
  purosuco build arquivo.suco        # Compila para binário executável nativo (.exe)
  purosuco c arquivo.suco            # Transpila para código C puro (.c)
  purosuco cs arquivo.suco           # Transpila para código C# (.g.cs)
  purosuco run-cs arquivo.suco       # Roda via .NET (C#)
  purosuco check arquivo.suco        # Análise semântica e erros
  purosuco formata arquivo.suco      # Formata o código PuroSuco
  purosuco tokens arquivo.suco       # Mostra tokens do Lexer
  purosuco ast arquivo.suco          # Mostra a Árvore Sintática (AST)
  purosuco install                   # Instala o purosuco no PATH do usuário

PuroSuco — a linguagem que compila o Brasil.
""");
}
