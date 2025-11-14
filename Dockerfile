# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia tudo de uma vez (simples, direto e funciona sempre)
COPY . .

# Restaura a solução inteira
RUN dotnet restore YourMoney.sln

# Publica só a API
RUN dotnet publish src/YourMoney.Api/YourMoney.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Porta obrigatória no Railway
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "YourMoney.Api.dll"]