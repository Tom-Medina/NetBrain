#!/bin/bash
APP_DIR=~/NetBrain/app
ZIP_URL="https://github.com/Tom-Medina/NetBrain/releases/download/latest/netbrain-linux-arm.zip"
TMP_ZIP="/tmp/netbrain-linux-arm.zip"

mkdir -p $APP_DIR

curl -sL "$ZIP_URL" -o "$TMP_ZIP"
unzip -o "$TMP_ZIP" -d "$APP_DIR"
rm "$TMP_ZIP"

pkill -f "dotnet NetBrain.dll" || true
sleep 1

cd $APP_DIR
chmod +x NetBrain
nohup dotnet NetBrain.dll > ~/NetBrain/log.txt 2>&1 &
echo "NetBrain updated and restarted." 
