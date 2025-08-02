FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY OrdrMate/*.csproj ./OrdrMate/
RUN dotnet restore OrdrMate/OrdrMate.csproj

WORKDIR /app/OrdrMate

ENTRYPOINT ["dotnet", "watch", "run", "--urls=http://0.0.0.0:5126"]