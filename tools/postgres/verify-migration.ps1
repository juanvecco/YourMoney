param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [string]$DockerContainer,
    [string]$Database = 'yourmoney_test'
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ManifestPath)) { throw 'Manifesto de origem não encontrado.' }
$manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json -AsHashtable
if ($DockerContainer) {
    $databaseUser = & docker exec $DockerContainer printenv POSTGRES_USER 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($databaseUser)) {
        throw 'Usuário PostgreSQL não encontrado no contêiner.'
    }
}
if ($manifest.BackupSqlVerify -ne 'Passed' -or -not $manifest.HostCopyHashMatches) {
    throw 'O manifesto não confirma backup verificado e cópia íntegra.'
}
if (-not $manifest.TableCounts -or -not $manifest.FinancialTotals) {
    throw 'O manifesto não contém contagens e totais.'
}

function Invoke-Query([string]$sql) {
    if ($DockerContainer) {
        $value = & docker exec $DockerContainer psql -U $databaseUser -d $Database -XAt -v ON_ERROR_STOP=1 -c $sql 2>$null
    } else {
        $value = & psql -XAt -v ON_ERROR_STOP=1 -c $sql 2>$null
    }
    if ($LASTEXITCODE -ne 0) {
        throw 'Falha ao consultar o PostgreSQL de destino. Confira conexão e esquema.'
    }
    return @($value | Where-Object { $_ -ne $null -and $_ -ne '' })
}

function Invoke-Scalar([string]$sql) {
    $rows = @(Invoke-Query $sql)
    if ($rows.Count -ne 1) { throw 'Consulta ao PostgreSQL não retornou um valor único.' }
    return [string]$rows[0]
}

function Assert-Identifier([string]$value) {
    if ($value -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
        throw 'Nome de tabela ou coluna inválido no manifesto.'
    }
}

$mismatches = [System.Collections.Generic.List[string]]::new()
$checkedTables = 0
foreach ($item in $manifest.TableCounts) {
    Assert-Identifier $item.Table
    $actual = Invoke-Scalar "SELECT COUNT(*) FROM public.`"$($item.Table)`";"
    $checkedTables++
    if ([long]$actual -ne [long]$item.Rows) {
        $mismatches.Add("$($item.Table): esperado $($item.Rows), obtido $actual")
    }
}

$culture = [System.Globalization.CultureInfo]::InvariantCulture
$checkedTotals = 0
foreach ($key in $manifest.FinancialTotals.Keys) {
    $parts = $key.Split('.')
    if ($parts.Count -ne 2) { throw 'Nome de total inválido no manifesto.' }
    Assert-Identifier $parts[0]
    Assert-Identifier $parts[1]
    $actual = Invoke-Scalar "SELECT COALESCE(SUM(`"$($parts[1])`"),0)::numeric(18,2) FROM public.`"$($parts[0])`";"
    $expectedDecimal = [decimal]::Parse([string]$manifest.FinancialTotals[$key], $culture)
    $actualDecimal = [decimal]::Parse($actual, $culture)
    $checkedTotals++
    if ($actualDecimal -ne $expectedDecimal) {
        $mismatches.Add("${key}: esperado $expectedDecimal, obtido $actualDecimal")
    }
}

$ownerTables = @(Invoke-Query "SELECT table_name FROM information_schema.columns WHERE table_schema='public' AND column_name='UsuarioId' ORDER BY table_name;")
foreach ($table in $ownerTables) {
    Assert-Identifier $table
    $orphans = Invoke-Scalar "SELECT COUNT(*) FROM public.`"$table`" f LEFT JOIN public.`"AspNetUsers`" u ON f.`"UsuarioId`" = u.`"Id`" WHERE f.`"UsuarioId`" IS NOT NULL AND u.`"Id`" IS NULL;"
    if ([long]$orphans -ne 0) { $mismatches.Add("${table}: $orphans registros sem usuário") }
}

if ($mismatches.Count -gt 0) {
    Write-Error "Migração divergente: $($mismatches -join '; ')"
    exit 1
}

Write-Output "Migração validada: $checkedTables tabelas, $checkedTotals totais exatos, 0 registros sem usuário em $($ownerTables.Count) tabelas do titular."
