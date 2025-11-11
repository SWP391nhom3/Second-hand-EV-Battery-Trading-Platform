# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/EVehicle.API/EVehicle.API.csproj", "src/EVehicle.API/"]
COPY ["src/EVehicle.Application/EVehicle.Application.csproj", "src/EVehicle.Application/"]
COPY ["src/EVehicle.Domain/EVehicle.Domain.csproj", "src/EVehicle.Domain/"]
COPY ["src/EVehicle.Infrastructure/EVehicle.Infrastructure.csproj", "src/EVehicle.Infrastructure/"]

RUN dotnet restore "src/EVehicle.API/EVehicle.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/EVehicle.API"
RUN dotnet build "EVehicle.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "EVehicle.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
EXPOSE 9290

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EVehicle.API.dll"]


