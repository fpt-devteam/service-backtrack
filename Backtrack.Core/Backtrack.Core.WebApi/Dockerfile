# ===================== Build stage =====================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files (from project root via railway setting)
COPY src/service-cv-management/Backtrack.Core.Domain/Backtrack.Core.Domain.csproj Backtrack.Core.Domain/
COPY src/service-cv-management/Backtrack.Core.Application/Backtrack.Core.Application.csproj Backtrack.Core.Application/
COPY src/service-cv-management/Backtrack.Core.Infrastructure/Backtrack.Core.Infrastructure.csproj Backtrack.Core.Infrastructure/
COPY src/service-cv-management/Backtrack.Core.WebApi/Backtrack.Core.WebApi.csproj Backtrack.Core.WebApi/

# Restore dependencies
RUN dotnet restore Backtrack.Core.WebApi/Backtrack.Core.WebApi.csproj

# Copy all the source code from service-cv-management
COPY ./src/service-cv-management/ ./

# Build and publish
RUN dotnet publish Backtrack.Core.WebApi/Backtrack.Core.WebApi.csproj \
  -c Release -o /app/publish /p:UseAppHost=false

# ===================== Runtime stage =====================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

# Listen on port 8080 (Railway automatically maps this)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Backtrack.Core.WebApi.dll"]
