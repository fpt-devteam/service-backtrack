# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Backtrack.ApiGateway/Backtrack.ApiGateway.csproj", "Backtrack.ApiGateway/"]
RUN dotnet restore "Backtrack.ApiGateway/Backtrack.ApiGateway.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Backtrack.ApiGateway"
RUN dotnet build "Backtrack.ApiGateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Backtrack.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Backtrack.ApiGateway.dll"]
