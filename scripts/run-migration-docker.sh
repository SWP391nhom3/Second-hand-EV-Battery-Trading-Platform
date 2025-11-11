#!/bin/bash

# Script to run migrations using Docker
# Usage: ./scripts/run-migration-docker.sh [command] [migration-name]
# Commands: add, update, list, script, remove

set -e

COMMAND=${1:-update}
MIGRATION_NAME=${2:-}

echo "=========================================="
echo "EVehicle Database Migration (Docker)"
echo "=========================================="
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Error: Docker is not running"
    exit 1
fi

# Check if SQL Server container is running
if ! docker-compose ps sqlserver | grep -q "Up"; then
    echo "Starting SQL Server container..."
    docker-compose up -d sqlserver
    echo "Waiting for SQL Server to be ready (30 seconds)..."
    sleep 30
fi

# Install EF Core tools in a temporary container and run migration
case $COMMAND in
    add)
        if [ -z "$MIGRATION_NAME" ]; then
            echo "Error: Migration name is required"
            echo "Usage: ./scripts/run-migration-docker.sh add MigrationName"
            exit 1
        fi
        echo "Creating migration: $MIGRATION_NAME"
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=EVehicleDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true" \
            --network evehicle_evehicle-network \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef
                export PATH=\"\$PATH:/root/.dotnet/tools\"
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
            -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=EVehicleDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true" \
            --network evehicle_evehicle-network \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef
                export PATH=\"\$PATH:/root/.dotnet/tools\"
                dotnet ef database update \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    list)
        echo "Listing migrations..."
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef
                export PATH=\"\$PATH:/root/.dotnet/tools\"
                dotnet ef migrations list \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    script)
        OUTPUT=${MIGRATION_NAME:-migration.sql}
        echo "Generating SQL script: $OUTPUT"
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef
                export PATH=\"\$PATH:/root/.dotnet/tools\"
                dotnet ef migrations script \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext \
                    --output /workspace/$OUTPUT
            "
        echo "SQL script saved to: $OUTPUT"
        ;;
    remove)
        echo "Removing last migration..."
        docker run --rm \
            -v "$(pwd):/workspace" \
            -w /workspace \
            mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
                dotnet tool install --global dotnet-ef
                export PATH=\"\$PATH:/root/.dotnet/tools\"
                dotnet ef migrations remove \
                    --project src/EVehicle.Infrastructure \
                    --startup-project src/EVehicle.API \
                    --context EVehicleDbContext
            "
        ;;
    *)
        echo "Error: Unknown command: $COMMAND"
        echo "Usage: ./scripts/run-migration-docker.sh [command] [migration-name]"
        echo "Commands: add, update, list, script, remove"
        exit 1
        ;;
esac

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Operation completed successfully!"
else
    echo ""
    echo "✗ Operation failed. Please check the error messages above."
    exit 1
fi

