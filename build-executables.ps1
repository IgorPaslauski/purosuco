# Build / Publish script for PuroSuco Executables (Windows win-x64)
Write-Host "Compilando executáveis do PuroSuco (CLI e LSP)..." -ForegroundColor Cyan

$distDir = Join-Path $PSScriptRoot "dist"
if (Test-Path $distDir) {
    Remove-Item $distDir -Recurse -Force
}

$cliOut = Join-Path $distDir "cli"
$lspOut = Join-Path $distDir "lsp"

# 1. Publicar CLI
dotnet publish "$PSScriptRoot/src/PuroSuco.Cli/PuroSuco.Cli.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -o $cliOut

# 2. Publicar Language Server
dotnet publish "$PSScriptRoot/src/PuroSuco.LanguageServer/PuroSuco.LanguageServer.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -o $lspOut

# Criar atalhos/cópias diretas com nomes padrão na raiz de dist/
Copy-Item (Join-Path $cliOut "PuroSuco.Cli.exe") (Join-Path $distDir "purosuco.exe") -Force
Copy-Item (Join-Path $lspOut "PuroSuco.LanguageServer.exe") (Join-Path $distDir "purosuco-lsp.exe") -Force

Write-Host "`nRECEBA! Executáveis gerados com sucesso em:" -ForegroundColor Green
Write-Host " -> $distDir\purosuco.exe (CLI)" -ForegroundColor Yellow
Write-Host " -> $distDir\purosuco-lsp.exe (LSP Server)" -ForegroundColor Yellow
