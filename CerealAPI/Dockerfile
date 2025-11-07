# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY *.sln .
COPY ["CerealAPI/CerealAPI.csproj", "."]
RUN dotnet restore

# Copy rest of source code and build
COPY CerealAPI/. ./CerealAPI
WORKDIR /source/CerealAPI
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CerealAPI.dll"]