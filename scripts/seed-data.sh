#!/bin/bash

# Script để seed dữ liệu ban đầu vào database
# Usage: ./scripts/seed-data.sh

set -e

cd "$(dirname "$0")/.."

echo "Đang seed dữ liệu vào database..."

# Chạy seed data thông qua dotnet run với một endpoint tạm thời
# Hoặc tạo một console app riêng để seed
# Ở đây ta sẽ sử dụng cách đơn giản: chạy ứng dụng và gọi seed method

# Export PATH để có thể sử dụng dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"

# Build project
echo "Đang build project..."
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Chạy migration nếu cần
echo "Kiểm tra migrations..."
dotnet ef database update --project src/EVehicle.Infrastructure --startup-project src/EVehicle.API

# Seed data sẽ được chạy tự động khi ứng dụng khởi động trong môi trường Development
# Hoặc có thể tạo một endpoint admin để trigger seed
echo "Seed data sẽ được chạy tự động khi ứng dụng khởi động (trong Development mode)."
echo "Hoặc có thể chạy ứng dụng và seed data sẽ được thực hiện tự động."

echo "Hoàn tất!"

