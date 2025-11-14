# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia só os csproj e restaura (camada de cache)
COPY *.sln .
COPY src/YourMoney.Api/YourMoney.Api.csproj ./src/YourMoney.Api/
COPY src/YourMoney.Application/YourMoney.Application.csproj ./src/YourMoney.Application/
COPY src/YourMoney.Domain/YourMoney.Domain.csproj ./src/YourMoney.Domain/
COPY src/YourMoney.Infrastructure/YourMoney.Infrastructure.csproj ./src/YourMoney.Infrastructure/

# Restaura pacotes (incluindo analyzers)
RUN dotnet restore src/YourMoney.Api/YourMoney.Api.csproj

# Copia todo o código
COPY . .

# Publica apenas o projeto da API (não a solução inteira!)
WORKDIR /src/src/YourMoney.Api
RUN dotnet publish YourMoney.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "YourMoney.Api.dll"]