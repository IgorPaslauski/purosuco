$ErrorActionPreference = "Stop"

Write-Host "🥤 Preparando PuroSuco..."

dotnet restore .\PuroSuco.sln
dotnet build .\PuroSuco.sln
dotnet test .\tests\PuroSuco.Core.Tests\PuroSuco.Core.Tests.csproj

Push-Location .\editor\purosuco-vscode
npm install
Pop-Location

Write-Host ""
Write-Host "RECEBA. Ambiente de desenvolvimento preparado."
Write-Host "Abra o projeto no VS Code/Cursor e rode a extensão em modo de desenvolvimento."
