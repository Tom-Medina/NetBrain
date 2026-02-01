#!/bin/bash
REPO_DIR=~/NetBrain/repo
APP_DIR=$REPO_DIR/publish

export PATH=$PATH:$HOME/.dotnet

cd $REPO_DIR
git pull

pkill -f "dotnet NetBrain.dll" || true
sleep 1

cd $APP_DIR
chmod +x NetBrain
cp ~/NetBrain/appsettings.json $APP_DIR/appsettings.json
nohup dotnet NetBrain.dll > ~/NetBrain/log.txt 2>&1 &
echo "NetBrain updated and restarted."
