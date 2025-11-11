#!/bin/bash

# Script để dừng EVehicle API đang chạy

echo "Đang tìm và dừng EVehicle API processes..."

# Tìm các process dotnet đang chạy EVehicle.API
PIDS=$(ps aux | grep -i "dotnet.*EVehicle.API" | grep -v grep | awk '{print $2}')

if [ -z "$PIDS" ]; then
    echo "Không tìm thấy process nào đang chạy."
    exit 0
fi

# Kill các process
for PID in $PIDS; do
    echo "Đang dừng process $PID..."
    kill -9 $PID 2>/dev/null || true
done

# Kiểm tra port 9190
PORT_PID=$(lsof -ti:9190 2>/dev/null)
if [ ! -z "$PORT_PID" ]; then
    echo "Đang giải phóng port 9190 (PID: $PORT_PID)..."
    kill -9 $PORT_PID 2>/dev/null || true
fi

sleep 1
echo "Đã dừng tất cả processes."

