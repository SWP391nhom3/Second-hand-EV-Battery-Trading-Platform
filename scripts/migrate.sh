#!/bin/bash

# Simple migration script
# Usage: ./scripts/migrate.sh [add|update] [migration-name]

set -e

COMMAND=${1:-update}
MIGRATION_NAME=${2:-InitialCreate}

echo "=========================================="
echo "EVehicle Database Migration"
echo "=========================================="
echo ""

# Check if SQL Server is running
if ! docker-compose ps sqlserver 2>/dev/null | grep -q "Up"; then
    echo "Starting SQL Server..."
    docker-compose up -d sqlserver
    echo "Waiting for SQL Server to be ready (30 seconds)..."
    sleep 30
fi

# Get the network name
NETWORK_NAME=$(docker inspect $(docker-compose ps -q sqlserver) --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}}{{end}}' 2>/dev/null | head -n1)

if [ -z "$NETWORK_NAME" ]; then
    echo "Error: Could not find SQL Server network"
    echo "Please ensure SQL Server container is running: docker-compose up -d sqlserver"
    exit 1
fi

echo "Using network: $NETWORK_NAME"
echo ""

case $COMMAND in
    add)
        echo "Creating migration: $MIGRATION_NAME"
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            --network "$NETWORK_NAME" \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef --verbosity quiet > /dev/null 2>&1
                export PATH=\"/root/.dotnet/tools:\$PATH\"
                dotnet ef migrations add $MIGRATION_NAME \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    update)
        echo "Applying migrations to database..."
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            --network "$NETWORK_NAME" \
            -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=EVehicleDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true" \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef --verbosity quiet > /dev/null 2>&1
                export PATH=\"/root/.dotnet/tools:\$PATH\"
                dotnet ef database update \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    *)
        echo "Error: Unknown command: $COMMAND"
        echo "Usage: ./scripts/migrate.sh [add|update] [migration-name]"
        exit 1
        ;;
esac

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Migration completed successfully!"
else
    echo ""
    echo "✗ Migration failed. Please check the error messages above."
    exit 1
fi

