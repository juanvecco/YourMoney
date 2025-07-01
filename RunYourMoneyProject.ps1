# Script PowerShell para executar e diagnosticar o projeto YourMoney
$ErrorActionPreference = "Stop"
$logFile = "YourMoneyDiagnostics.log"
$projectPath = "src\YourMoney.Api"
$slnPath = "YourMoney.sln"
$apiUrl = "https://localhost:60915" # Ajuste a porta se necessário (verifique a saída do dotnet run)

# Função para escrever logs no console e arquivo
function Write-Log {
    param($Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage
}

# Iniciar log
Write-Log "Iniciando diagnóstico do projeto YourMoney..."

# 1. Verificar .NET SDK
Write-Log "Verificando .NET SDK..."
try {
    $dotnetVersion = dotnet --version
    Write-Log ".NET SDK versão: $dotnetVersion"
    if ($dotnetVersion -notlike "8.*") {
        Write-Log "AVISO: O projeto requer .NET 8.0. Verifique a instalação."
    }
} catch {
    Write-Log "ERRO: .NET SDK não encontrado. Instale o .NET 8.0 em https://dotnet.microsoft.com/download"
    exit 1
}

# 2. Verificar SQL Server
Write-Log "Verificando conexão com SQL Server..."
try {
    $sqlConnectionString = "Server=localhost,1433;Database=master;User Id=sa;Password=Top@2024!;TrustServerCertificate=True;"
    $sqlCommand = "SELECT 1"
    $connection = New-Object System.Data.SqlClient.SqlConnection($sqlConnectionString)
    $connection.Open()
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlCommand, $connection)
    $result = $command.ExecuteScalar()
    Write-Log "Conexão com SQL Server bem-sucedida."
    $connection.Close()
} catch {
    Write-Log "ERRO: Falha ao conectar ao SQL Server. Verifique se o servidor está rodando e as credenciais (sa/Top@2024!). Detalhes: $_"
}

# 3. Restaurar dependências
Write-Log "Restaurando dependências..."
try {
    dotnet restore $slnPath | Out-File -Append -FilePath $logFile
    Write-Log "Dependências restauradas com sucesso."
} catch {
    Write-Log "ERRO: Falha ao restaurar dependências. Verifique o log para detalhes."
    exit 1
}

# 4. Compilar o projeto
Write-Log "Compilando o projeto..."
try {
    dotnet build $slnPath --no-restore | Out-File -Append -FilePath $logFile
    Write-Log "Compilação concluída com sucesso."
} catch {
    Write-Log "ERRO: Falha na compilação. Verifique o log para detalhes."
    exit 1
}

# 5. Verificar/Applying migrações
Write-Log "Verificando migrações do Entity Framework..."
try {
    Set-Location $projectPath
    dotnet ef migrations add InitialCreate --project ..\YourMoney.Infrastructure --startup-project . --no-build --if-not-exists | Out-File -Append -FilePath "..\..\YourMoneyDiagnostics.log"
    Write-Log "Migração 'InitialCreate' adicionada (se necessário)."
    dotnet ef database update --project ..\YourMoney.Infrastructure --startup-project . --no-build | Out-File -Append -FilePath "..\..\YourMoneyDiagnostics.log"
    Write-Log "Banco de dados atualizado com sucesso."
    Set-Location ..\..
} catch {
    Write-Log "ERRO: Falha ao aplicar migrações. Verifique o log para detalhes."
}

# 6. Executar o projeto
Write-Log "Iniciando a API..."
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project $projectPath" -PassThru -NoNewWindow
Start-Sleep -Seconds 10 # Aguarda a API iniciar

# 7. Testar a API
Write-Log "Testando a API em $apiUrl..."
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/swagger/index.html" -SkipCertificateCheck
    Write-Log "API está acessível. Swagger respondeu com status: $($response.StatusCode)"
} catch {
    Write-Log "ERRO: Falha ao acessar a API. Detalhes: $_"
}

# 8. Testar o frontend (se wwwroot/index.html existir)
if (Test-Path "$projectPath\wwwroot\index.html") {
    Write-Log "Testando o frontend em $apiUrl/index.html..."
    try {
        $response = Invoke-WebRequest -Uri "$apiUrl/index.html" -SkipCertificateCheck
        Write-Log "Frontend está acessível. Resposta com status: $($response.StatusCode)"
    } catch {
        Write-Log "ERRO: Falha ao acessar o frontend. Detalhes: $_"
    }
} else {
    Write-Log "AVISO: Arquivo wwwroot/index.html não encontrado. Frontend ainda não configurado."
}

# 9. Finalizar
Write-Log "Diagnóstico concluído. Verifique o arquivo $logFile para detalhes."
Write-Log "Para parar a API, pressione Ctrl+C ou feche o terminal."

# Manter o processo rodando até o usuário interromper
try {
    Wait-Process -Id $process.Id
} catch {
    Write-Log "API encerrada."
}