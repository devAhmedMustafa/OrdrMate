FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5126

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrdrMate/*.csproj ./OrdrMate/
RUN dotnet restore OrdrMate/OrdrMate.csproj

COPY . .
WORKDIR /src/OrdrMate

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["bash", "-c", "\
    dotnet ef database update && \
    dotnet watch run --urls=http://0.0.0.0:5126"]
