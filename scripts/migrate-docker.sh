#!/bin/bash

# Script to create and apply migrations using Docker
# Usage: ./scripts/migrate-docker.sh [migration-name]

set -e

MIGRATION_NAME=${1:-InitialCreate}

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

echo "Step 1: Building API image (if needed)..."
docker-compose build api

echo ""
echo "Step 2: Creating migration: $MIGRATION_NAME"
docker-compose run --rm api dotnet ef migrations add $MIGRATION_NAME \
    --project src/EVehicle.Infrastructure \
    --startup-project src/EVehicle.API \
    --context EVehicleDbContext

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Migration created successfully!"
    echo ""
    echo "Step 3: Applying migration to database..."
    docker-compose run --rm api dotnet ef database update \
        --project src/EVehicle.Infrastructure \
        --startup-project src/EVehicle.API \
        --context EVehicleDbContext
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "✓ Migration applied successfully!"
        echo "Database is ready to use."
        echo ""
        echo "You can now start the API:"
        echo "  docker-compose up -d api"
    else
        echo ""
        echo "✗ Failed to apply migration. Please check the error messages above."
        exit 1
    fi
else
    echo ""
    echo "✗ Failed to create migration. Please check the error messages above."
    exit 1
fi

