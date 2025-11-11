#!/bin/bash

# Script to initialize database with migrations
# Usage: ./scripts/init-db.sh

set -e

cd "$(dirname "$0")/.."

# Add dotnet tools to PATH
export PATH="$PATH:$HOME/.dotnet/tools"

echo "Checking dotnet-ef tool..."
if ! command -v dotnet-ef &> /dev/null; then
    echo "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

echo "Waiting for SQL Server to be ready..."
sleep 5

echo "Running database migrations..."
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

echo "Database initialization completed!"

