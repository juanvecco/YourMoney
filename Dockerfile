# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia a solução e todos os csproj
COPY *.sln .
COPY src/**/*.csproj ./src/
# Ajusta o caminho dos projetos para o formato correto dentro do container
RUN find src -name "*.csproj" -exec dirname {} \; | sed 's/^src\///' | awk '{print "\""$0"/"$0".csproj\"","\"src/"$0"\""}' | sed 's/.*/COPY [&] .\//' > /tmp/copy.proj
RUN cat /tmp/copy.proj | xargs -I {} sh -c "{}"

# Restaura com o caminho correto (OBRIGATÓRIO)
RUN dotnet restore YourMoney.sln

# Copia todo o código fonte
COPY . .

# Publica a API (sem --no-restore!)
WORKDIR /src/src/YourMoney.Api
RUN dotnet publish YourMoney.Api.csproj -c Release -o /app/publish

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "YourMoney.Api.dll"]