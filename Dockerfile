FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for better caching
COPY ["GymBrain.sln", "./"]
COPY ["src/GymBrain.Domain/GymBrain.Domain.csproj", "src/GymBrain.Domain/"]
COPY ["src/GymBrain.Application/GymBrain.Application.csproj", "src/GymBrain.Application/"]
COPY ["src/GymBrain.Infrastructure/GymBrain.Infrastructure.csproj", "src/GymBrain.Infrastructure/"]
COPY ["src/GymBrain.Api/GymBrain.Api.csproj", "src/GymBrain.Api/"]

# Copy test projects to satisfy solution-level restore
COPY ["tests/GymBrain.Domain.Tests/GymBrain.Domain.Tests.csproj", "tests/GymBrain.Domain.Tests/"]
COPY ["tests/GymBrain.Application.Tests/GymBrain.Application.Tests.csproj", "tests/GymBrain.Application.Tests/"]
COPY ["tests/GymBrain.Infrastructure.Tests/GymBrain.Infrastructure.Tests.csproj", "tests/GymBrain.Infrastructure.Tests/"]
COPY ["tests/GymBrain.Api.Tests/GymBrain.Api.Tests.csproj", "tests/GymBrain.Api.Tests/"]

RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR "/src/src/GymBrain.Api"
RUN dotnet build "GymBrain.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GymBrain.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cloud Run sets the PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GymBrain.Api.dll"]
