#!/bin/bash

# Script to apply database migrations
# Usage: ./scripts/apply-migration.sh

set -e

echo "=========================================="
echo "Applying Database Migration"
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

echo "Applying migrations to database..."
echo "Make sure SQL Server is running and connection string is correct in appsettings.json"
echo ""

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
