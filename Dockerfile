FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore the production dependency graph with the same shared build settings.
COPY ["Directory.Build.props", "./"]
COPY ["src/GymBrain.Domain/GymBrain.Domain.csproj", "src/GymBrain.Domain/"]
COPY ["src/GymBrain.Application/GymBrain.Application.csproj", "src/GymBrain.Application/"]
COPY ["src/GymBrain.Infrastructure/GymBrain.Infrastructure.csproj", "src/GymBrain.Infrastructure/"]
COPY ["src/GymBrain.Api/GymBrain.Api.csproj", "src/GymBrain.Api/"]
RUN dotnet restore "src/GymBrain.Api/GymBrain.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/GymBrain.Api/GymBrain.Api.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway's service target port must match this listener.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GymBrain.Api.dll"]
