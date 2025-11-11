#!/bin/bash

# Script to create and apply database migrations
# Usage: ./scripts/migrate-db.sh [migration-name]

set -e

MIGRATION_NAME=${1:-InitialCreate}

echo "=========================================="
echo "EVehicle Database Migration Script"
echo "=========================================="
echo ""

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    if [ -f "/usr/local/share/dotnet/dotnet" ]; then
        export PATH="/usr/local/share/dotnet:$PATH"
    else
        echo "Error: dotnet CLI is not installed or not in PATH"
        echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
        exit 1
    fi
fi

echo "Step 1: Restoring NuGet packages..."
dotnet restore

echo ""
echo "Step 2: Building solution..."
dotnet build

echo ""
echo "Step 3: Creating migration: $MIGRATION_NAME"
dotnet ef migrations add $MIGRATION_NAME \
    --project src/EVehicle.Infrastructure \
    --startup-project src/EVehicle.API \
    --context EVehicleDbContext

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Migration created successfully!"
    echo ""
    echo "Step 4: Applying migration to database..."
    echo "Make sure SQL Server is running and connection string is correct in appsettings.json"
    echo ""
    read -p "Do you want to apply the migration now? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        dotnet ef database update \
            --project src/EVehicle.Infrastructure \
            --startup-project src/EVehicle.API \
            --context EVehicleDbContext
        
        if [ $? -eq 0 ]; then
            echo ""
            echo "✓ Migration applied successfully!"
            echo "Database is ready to use."
        else
            echo ""
            echo "✗ Failed to apply migration. Please check the error messages above."
            exit 1
        fi
    else
        echo ""
        echo "Migration created but not applied."
        echo "To apply later, run:"
        echo "  dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API"
    fi
else
    echo ""
    echo "✗ Failed to create migration. Please check the error messages above."
    exit 1
fi
