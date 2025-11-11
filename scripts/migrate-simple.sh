#!/bin/bash

# Simple migration script using docker-compose
# Usage: ./scripts/migrate-simple.sh [add|update] [migration-name]

set -e

COMMAND=${1:-update}
MIGRATION_NAME=${2:-InitialCreate}

echo "=========================================="
echo "EVehicle Database Migration"
echo "=========================================="
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Error: Docker is not running"
    exit 1
fi

# Ensure SQL Server is running
if ! docker-compose ps sqlserver | grep -q "Up"; then
    echo "Starting SQL Server..."
    docker-compose up -d sqlserver
    echo "Waiting for SQL Server to be ready (30 seconds)..."
    sleep 30
fi

# Build a temporary container with .NET SDK and EF tools
echo "Setting up migration environment..."

case $COMMAND in
    add)
        echo "Creating migration: $MIGRATION_NAME"
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            --network "$(docker-compose ps -q sqlserver | xargs docker inspect --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}}{{end}}' 2>/dev/null | head -n1)" \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef --verbosity quiet
                export PATH=\"/root/.dotnet/tools:\$PATH\"
                dotnet ef migrations add $MIGRATION_NAME \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    update)
        echo "Applying migrations to database..."
        # Use sqlserver hostname from docker-compose network
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            --network "$(docker-compose ps -q sqlserver | xargs docker inspect --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}}{{end}}' 2>/dev/null | head -n1)" \
            -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=EVehicleDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true" \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef --verbosity quiet
                export PATH=\"/root/.dotnet/tools:\$PATH\"
                dotnet ef database update \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    *)
        echo "Error: Unknown command: $COMMAND"
        echo "Usage: ./scripts/migrate-simple.sh [add|update] [migration-name]"
        exit 1
        ;;
esac

echo ""
echo "✓ Operation completed successfully!"

