# Use a imagem base do .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências para otimizar o cache
COPY ["src/YourMoney.Api/YourMoney.Api.csproj", "src/YourMoney.Api/"]
COPY ["src/YourMoney.Application/YourMoney.Application.csproj", "src/YourMoney.Application/"]
COPY ["src/YourMoney.Domain/YourMoney.Domain.csproj", "src/YourMoney.Domain/"]
COPY ["src/YourMoney.Infrastructure/YourMoney.Infrastructure.csproj", "src/YourMoney.Infrastructure/"]

# Restaura as dependências
RUN dotnet restore "src/YourMoney.Api/YourMoney.Api.csproj"

# Copia todo o código fonte e constrói o projeto
COPY . .
WORKDIR "/src/src/YourMoney.Api"
RUN dotnet build "YourMoney.Api.csproj" -c Release -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "YourMoney.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use a imagem base do ASP.NET Runtime para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourMoney.Api.dll"]