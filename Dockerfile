# Stage 1: Base runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Stage 2: Build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for layer caching
COPY ["RecyclingApp.sln", "."]
COPY ["RecyclingApp.API/RecyclingApp.API.csproj", "RecyclingApp.API/"]
COPY ["RecyclingApp.Application/RecyclingApp.Application.csproj", "RecyclingApp.Application/"]
COPY ["RecyclingApp.Domain/RecyclingApp.Domain.csproj", "RecyclingApp.Domain/"]
COPY ["RecyclingApp.Infrastructure/RecyclingApp.Infrastructure.csproj", "RecyclingApp.Infrastructure/"]

# Restore packages
RUN dotnet restore "./RecyclingApp.sln"

# Copy the remaining source code
COPY . .

# Build the API project
WORKDIR "/src/RecyclingApp.API"
RUN dotnet build "./RecyclingApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RecyclingApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecyclingApp.API.dll"]
