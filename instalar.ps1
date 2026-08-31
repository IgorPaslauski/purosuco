# Script de instalação do PuroSuco no PATH global do Windows
Write-Host "🥤 Compilando PuroSuco como executável autônomo (Self-Contained Single-File)..." -ForegroundColor Cyan

$userProfile = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::UserProfile)
$installDir = Join-Path $userProfile ".purosuco\bin"

if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

$cliProj = Join-Path $PSScriptRoot "src\PuroSuco.Cli\PuroSuco.Cli.csproj"
$tempPublish = Join-Path $PSScriptRoot "dist\self-contained"

# 1. Publicar purosuco.exe autônomo (não precisa do SDK .NET instalado para rodar)
dotnet publish $cliProj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $tempPublish

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao compilar o executável do PuroSuco."
    exit 1
}

# 2. Copiar para o diretório de instalação ~/.purosuco/bin/
$targetExe = Join-Path $installDir "purosuco.exe"
Copy-Item (Join-Path $tempPublish "PuroSuco.Cli.exe") $targetExe -Force

# 3. Adicionar ~/.purosuco/bin ao PATH do usuário
$currentPath = [System.Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::User)
$paths = $currentPath -split ';' | Where-Object { $_ -ne "" }

if ($paths -notcontains $installDir) {
    $newPath = ($paths + $installDir) -join ';'
    [System.Environment]::SetEnvironmentVariable("Path", $newPath, [System.EnvironmentVariableTarget]::User)
    Write-Host "Adicionado ao PATH de Usuário: $installDir" -ForegroundColor Green
} else {
    Write-Host "O diretório $installDir já estava no PATH." -ForegroundColor Gray
}

# Atualizar o PATH da sessão atual do PowerShell para testar na hora
$env:Path = "$installDir;$env:Path"

Write-Host "`nRECEBA! PuroSuco instalado com sucesso!" -ForegroundColor Green
Write-Host "Local: $targetExe" -ForegroundColor Yellow
Write-Host "`nAgora você pode abrir qualquer terminal e rodar:" -ForegroundColor Cyan
Write-Host "  purosuco run examples/hello.suco" -ForegroundColor White
Write-Host "  purosuco build examples/calculadora.suco" -ForegroundColor White
