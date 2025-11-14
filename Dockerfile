# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore YourMoney.sln
RUN dotnet publish src/YourMoney.Api/YourMoney.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# OBRIGATÓRIO no Railway com Private Networking
EXPOSE 8080

# ← comentário em linha separada (o problema era isso)
ENV ASPNETCORE_URLS=http://[::]:8080

ENTRYPOINT ["dotnet", "YourMoney.Api.dll"]