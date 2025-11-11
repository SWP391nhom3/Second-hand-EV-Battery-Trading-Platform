#!/bin/bash

# Script để rebuild và restart backend sau khi cập nhật Payment Gateway

echo "🔄 Rebuilding Backend..."

# Check if Docker is running
if docker ps > /dev/null 2>&1; then
    echo "📦 Docker is running. Rebuilding Docker containers..."
    
    # Stop containers
    echo "⏹️  Stopping containers..."
    docker-compose down
    
    # Rebuild without cache
    echo "🔨 Rebuilding images (no cache)..."
    docker-compose build --no-cache api
    
    # Start containers
    echo "🚀 Starting containers..."
    docker-compose up -d
    
    # Show logs
    echo "📋 Showing API logs..."
    docker-compose logs -f api
else
    echo "💻 Docker is not running. Building locally..."
    
    # Clean
    echo "🧹 Cleaning solution..."
    dotnet clean
    
    # Build
    echo "🔨 Building solution..."
    dotnet build
    
    if [ $? -eq 0 ]; then
        echo "✅ Build successful!"
        echo "🚀 To run the API, use: dotnet run --project src/EVehicle.API"
    else
        echo "❌ Build failed! Please check the errors above."
        exit 1
    fi
fi

echo "✅ Done!"

