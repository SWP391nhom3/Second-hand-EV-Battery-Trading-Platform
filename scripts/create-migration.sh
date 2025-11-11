#!/bin/bash

# Script to create a new migration
# Usage: ./scripts/create-migration.sh MigrationName

if [ -z "$1" ]; then
    echo "Error: Migration name is required"
    echo "Usage: ./scripts/create-migration.sh MigrationName"
    exit 1
fi

MIGRATION_NAME=$1

echo "Creating migration: $MIGRATION_NAME"
dotnet ef migrations add $MIGRATION_NAME --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

echo "Migration created successfully!"

