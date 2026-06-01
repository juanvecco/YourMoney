param(
    [string] $ConnectionString,

    [string] $OwnerUserId = '3c3e04ec-651e-4de3-8e7a-5dc6f47c2a10',

    [string] $OtherUserId = '76030764-b886-478d-8ea4-9cb5fd46041d',

    [int] $Mes = 5,

    [int] $Ano = 2026
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiSettingsPath = Join-Path $repoRoot 'src/YourMoney.Api/appsettings.json'

function Get-ConfiguredConnectionString {
    if ($ConnectionString) { return $ConnectionString }
    if ($env:DATABASE_CONNECTION_STRING) { return $env:DATABASE_CONNECTION_STRING }

    $settings = Get-Content -LiteralPath $apiSettingsPath -Raw | ConvertFrom-Json
    return $settings.ConnectionStrings.DefaultConnection
}

function Convert-ConnectionString {
    param([string] $RawConnectionString)

    $parts = @{}
    foreach ($segment in $RawConnectionString.Split(';')) {
        if ([string]::IsNullOrWhiteSpace($segment) -or -not $segment.Contains('=')) { continue }
        $key, $value = $segment.Split('=', 2)
        $parts[$key.Trim().ToLowerInvariant()] = $value.Trim()
    }

    $server = $parts['server']
    if (-not $server) { $server = $parts['data source'] }

    $database = $parts['database']
    if (-not $database) { $database = $parts['initial catalog'] }

    [pscustomobject]@{
        Server = $server
        Database = $database
        User = $parts['user id']
        Password = $parts['password']
    }
}

$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if (-not $sqlcmd) {
    throw 'sqlcmd was not found. Install SQL Server command line tools or run on a machine with sqlcmd.'
}

$connection = Convert-ConnectionString (Get-ConfiguredConnectionString)
$query = @"
DECLARE @OwnerUserId nvarchar(450) = N'$OwnerUserId';
DECLARE @OtherUserId nvarchar(450) = N'$OtherUserId';
DECLARE @Mes int = $Mes;
DECLARE @Ano int = $Ano;

SELECT 'AspNetUsers' AS Area, Id, Email
FROM dbo.AspNetUsers
WHERE Id IN (@OwnerUserId, @OtherUserId)
ORDER BY Email;

SELECT 'OwnerPeriod' AS Area, 'tbDespesa' AS TableName, COUNT_BIG(*) AS Rows
FROM dbo.tbDespesa
WHERE UsuarioId = @OwnerUserId AND MONTH(Data) = @Mes AND YEAR(Data) = @Ano
UNION ALL
SELECT 'OwnerPeriod', 'tbReceita', COUNT_BIG(*)
FROM dbo.tbReceita
WHERE UsuarioId = @OwnerUserId AND MONTH(Data) = @Mes AND YEAR(Data) = @Ano
UNION ALL
SELECT 'OwnerPeriod', 'tbInvestimento', COUNT_BIG(*)
FROM dbo.tbInvestimento
WHERE UsuarioId = @OwnerUserId AND MONTH(DataInvestimento) = @Mes AND YEAR(DataInvestimento) = @Ano
UNION ALL
SELECT 'OtherUserPeriod', 'tbDespesa', COUNT_BIG(*)
FROM dbo.tbDespesa
WHERE UsuarioId = @OtherUserId AND MONTH(Data) = @Mes AND YEAR(Data) = @Ano
UNION ALL
SELECT 'OtherUserPeriod', 'tbReceita', COUNT_BIG(*)
FROM dbo.tbReceita
WHERE UsuarioId = @OtherUserId AND MONTH(Data) = @Mes AND YEAR(Data) = @Ano
UNION ALL
SELECT 'OtherUserPeriod', 'tbInvestimento', COUNT_BIG(*)
FROM dbo.tbInvestimento
WHERE UsuarioId = @OtherUserId AND MONTH(DataInvestimento) = @Mes AND YEAR(DataInvestimento) = @Ano;
"@

$args = @('-S', $connection.Server, '-d', $connection.Database, '-b', '-r', '1', '-Q', $query)
if ($connection.User) {
    $args += @('-U', $connection.User, '-P', $connection.Password)
}
else {
    $args += '-E'
}

& $sqlcmd.Source @args
if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd exited with code $LASTEXITCODE."
}
