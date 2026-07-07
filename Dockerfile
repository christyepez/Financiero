FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Financiero.Api/Financiero.Api.csproj
RUN dotnet publish src/Financiero.Api/Financiero.Api.csproj -c Release --no-restore -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Financiero.Api.dll"]
