#!/bin/bash

# Script để chạy EVehicle API với Production environment trên port 9290

cd "$(dirname "$0")/.."

echo "=========================================="
echo "EVehicle API - Production Mode"
echo "=========================================="
echo ""

# Kiểm tra database connection
echo "Kiểm tra kết nối database..."
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API --no-build > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "⚠️  Warning: Có thể có vấn đề với database connection"
    echo "   Đảm bảo SQL Server đang chạy và connection string đúng"
    echo ""
fi

echo "Đang build project (Release mode)..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "=========================================="
echo "Đang khởi động API Production..."
echo "Environment: Production"
echo "Port: 9290"
echo "URL: http://localhost:9290"
echo "Swagger: http://localhost:9290/swagger"
echo "=========================================="
echo ""
echo "Nhấn Ctrl+C để dừng"
echo ""

# Chạy với Production environment
# Không sử dụng launch profile để tránh override từ launchSettings.json
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:9290
dotnet run --project src/EVehicle.API --configuration Release --no-build --no-launch-profile

