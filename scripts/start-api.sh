#!/bin/bash

# Script để chạy EVehicle API với hot reload

cd "$(dirname "$0")/.."

echo "Đang build project..."
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Đang chạy EVehicle API trên port 9190 với hot reload..."
echo "Hot reload: Thay đổi code sẽ tự động reload ứng dụng"
echo "Nhấn Ctrl+C để dừng"
echo ""

# Sử dụng dotnet watch để enable hot reload
# dotnet watch sẽ tự động:
# - Watch file changes và rebuild khi cần
# - Hot reload application khi có thay đổi (không cần restart)
# - Restart application nếu hot reload không thể áp dụng

dotnet watch run --project src/EVehicle.API/EVehicle.API.csproj --launch-profile http

