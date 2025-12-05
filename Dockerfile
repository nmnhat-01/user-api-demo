# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["UserApiDemo.sln", "."]
COPY ["UserApiDemo.API/UserApiDemo.API.csproj", "UserApiDemo.API/"]
COPY ["UserApiDemo.Domain/UserApiDemo.Domain.csproj", "UserApiDemo.Domain/"]
COPY ["UserApiDemo.Application/UserApiDemo.Application.csproj", "UserApiDemo.Application/"]
COPY ["UserApiDemo.Infrastructure/UserApiDemo.Infrastructure.csproj", "UserApiDemo.Infrastructure/"]
COPY ["UserApiDemo.Persistence/UserApiDemo.Persistence.csproj", "UserApiDemo.Persistence/"]

# Restore NuGet packages
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserApiDemo.API/UserApiDemo.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "UserApiDemo.API.dll"]
