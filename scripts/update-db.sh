#!/bin/bash

# Script to update database with latest migrations
# Usage: ./scripts/update-db.sh

echo "Updating database..."
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

echo "Database updated successfully!"

