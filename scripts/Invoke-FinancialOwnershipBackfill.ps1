param(
    [ValidateSet('EnsureSchema', 'Validate', 'Execute')]
    [string] $Mode = 'Validate',

    [string] $ConnectionString,

    [string] $TargetUserId = '3c3e04ec-651e-4de3-8e7a-5dc6f47c2a10',

    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiSettingsPath = Join-Path $repoRoot 'src/YourMoney.Api/appsettings.json'
$validateScript = Join-Path $repoRoot 'src/YourMoney.Infrastructure/Scripts/ValidateFinancialOwnership.sql'
$backfillScript = Join-Path $repoRoot 'src/YourMoney.Infrastructure/Scripts/BackfillFinancialOwnershipToTargetUser.sql'
$ensureSchemaScript = Join-Path $repoRoot 'src/YourMoney.Infrastructure/Scripts/EnsureFinancialOwnershipSchema.sql'

function Get-ConfiguredConnectionString {
    if ($ConnectionString) {
        return $ConnectionString
    }

    if ($env:DATABASE_CONNECTION_STRING) {
        return $env:DATABASE_CONNECTION_STRING
    }

    if (-not (Test-Path -LiteralPath $apiSettingsPath)) {
        throw "Could not find appsettings.json at $apiSettingsPath."
    }

    $settings = Get-Content -LiteralPath $apiSettingsPath -Raw | ConvertFrom-Json
    $configured = $settings.ConnectionStrings.DefaultConnection

    if (-not $configured) {
        throw 'DefaultConnection was not found in appsettings.json.'
    }

    return $configured
}

function Convert-ConnectionString {
    param([string] $RawConnectionString)

    $parts = @{}
    foreach ($segment in $RawConnectionString.Split(';')) {
        if ([string]::IsNullOrWhiteSpace($segment) -or -not $segment.Contains('=')) {
            continue
        }

        $key, $value = $segment.Split('=', 2)
        $parts[$key.Trim().ToLowerInvariant()] = $value.Trim()
    }

    $server = $parts['server']
    if (-not $server) { $server = $parts['data source'] }

    $database = $parts['database']
    if (-not $database) { $database = $parts['initial catalog'] }

    $user = $parts['user id']
    if (-not $user) { $user = $parts['uid'] }

    $password = $parts['password']
    if (-not $password) { $password = $parts['pwd'] }

    if (-not $server -or -not $database) {
        throw 'Connection string must include Server and Database.'
    }

    [pscustomobject]@{
        Server = $server
        Database = $database
        User = $user
        Password = $password
        IntegratedSecurity = ($parts['integrated security'] -in @('true', 'sspi'))
    }
}

function Invoke-SqlFile {
    param(
        [string] $ScriptPath,
        [string] $DryRun
    )

    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if (-not $sqlcmd) {
        throw 'sqlcmd was not found. Install SQL Server command line tools or run this script on a machine with sqlcmd.'
    }

    $connection = Convert-ConnectionString (Get-ConfiguredConnectionString)

    $args = @(
        '-S', $connection.Server,
        '-d', $connection.Database,
        '-b',
        '-r', '1',
        '-i', $ScriptPath,
        '-v', "TargetUserId=$TargetUserId", "DryRun=$DryRun"
    )

    if ($connection.User) {
        $args += @('-U', $connection.User, '-P', $connection.Password)
    }
    else {
        $args += '-E'
    }

    $output = & $sqlcmd.Source @args 2>&1
    $exitCode = $LASTEXITCODE
    $text = ($output | Out-String).TrimEnd()

    if ($OutputPath) {
        $directory = Split-Path -Parent $OutputPath
        if ($directory) {
            New-Item -ItemType Directory -Force -Path $directory | Out-Null
        }

        Set-Content -LiteralPath $OutputPath -Value $text -Encoding UTF8
    }

    Write-Output $text

    if ($exitCode -ne 0) {
        throw "sqlcmd exited with code $exitCode."
    }
}

if ($Mode -eq 'EnsureSchema') {
    Invoke-SqlFile -ScriptPath $ensureSchemaScript -DryRun '1'
}
elseif ($Mode -eq 'Validate') {
    Invoke-SqlFile -ScriptPath $validateScript -DryRun '1'
}
else {
    Invoke-SqlFile -ScriptPath $backfillScript -DryRun '0'
}
