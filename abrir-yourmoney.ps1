[CmdletBinding()]
param(
    [string]$FrontendPath = (Join-Path (Split-Path -Parent $PSScriptRoot) "YourMoney-angular"),
    [switch]$SkipMigrations
)

$ErrorActionPreference = "Stop"

$solutionRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$apiProject = Join-Path $solutionRoot "src\YourMoney.Api\YourMoney.Api.csproj"
$apiExecutableRoot = [IO.Path]::GetFullPath((Join-Path $solutionRoot "src\YourMoney.Api\bin"))
$frontendRoot = [IO.Path]::GetFullPath($FrontendPath)
$angularOutput = Join-Path $frontendRoot "dist\yourmoney-app\browser"
$wwwroot = Join-Path $solutionRoot "src\YourMoney.Api\wwwroot"

function Stop-ExistingApi {
    $processes = Get-Process -Name "YourMoney.Api" -ErrorAction SilentlyContinue

    foreach ($process in $processes) {
        try {
            $resolvedProcessPath = [IO.Path]::GetFullPath($process.Path)
            if ($resolvedProcessPath.StartsWith($apiExecutableRoot, [StringComparison]::OrdinalIgnoreCase)) {
                Stop-Process -Id $process.Id -Force
                Write-Host "API anterior encerrada (PID $($process.Id))."
            }
        }
        catch {
            Write-Warning "Não foi possível verificar o processo $($process.Id): $($_.Exception.Message)"
        }
    }
}

function Publish-AngularToApi {
    if (-not (Test-Path -LiteralPath (Join-Path $frontendRoot "package.json"))) {
        throw "Frontend Angular não encontrado em: $frontendRoot"
    }

    Push-Location $frontendRoot
    try {
        & npm.cmd ci
        if ($LASTEXITCODE -ne 0) { throw "npm ci falhou com código $LASTEXITCODE" }

        & npm.cmd run build
        if ($LASTEXITCODE -ne 0) { throw "npm run build falhou com código $LASTEXITCODE" }
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path -LiteralPath $angularOutput)) {
        throw "Build Angular não encontrado em: $angularOutput"
    }

    New-Item -ItemType Directory -Path $wwwroot -Force | Out-Null
    Get-ChildItem -LiteralPath $wwwroot -Force | Remove-Item -Recurse -Force
    Copy-Item -Path (Join-Path $angularOutput "*") -Destination $wwwroot -Recurse -Force
}

function Update-Database {
    & dotnet ef database update `
        --project (Join-Path $solutionRoot "src\YourMoney.Infrastructure\YourMoney.Infrastructure.csproj") `
        --startup-project $apiProject `
        --context AppDbContext
    if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar o banco financeiro." }

    & dotnet ef database update `
        --project (Join-Path $solutionRoot "src\YourMoney.Infrastructure\YourMoney.Infrastructure.csproj") `
        --startup-project $apiProject `
        --context ApplicationDbContext
    if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar o banco de identidade." }
}

function Start-Api {
    return Start-Process dotnet `
        -ArgumentList @("run", "--project", $apiProject) `
        -WorkingDirectory $solutionRoot `
        -PassThru
}

Stop-ExistingApi
Publish-AngularToApi
if (-not $SkipMigrations) {
    Update-Database
}
$apiProcess = Start-Api

Write-Host "YourMoney iniciado (PID $($apiProcess.Id))."
Write-Host "- Frontend: https://localhost:5001/dashboard"
Write-Host "- API Swagger: https://localhost:5001/swagger"

Start-Process "https://localhost:5001/dashboard"
