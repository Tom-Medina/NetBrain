#!/bin/bash
REPO_DIR=~/NetBrain/repo
APP_DIR=~/NetBrain/publish

export PATH=$PATH:$HOME/.dotnet

cd $REPO_DIR
git pull

dotnet publish -c Release -r linux-arm -o $APP_DIR

pkill -f "dotnet NetBrain.dll" || true
sleep 1

cd $APP_DIR
chmod +x NetBrain
nohup dotnet NetBrain.dll > ~/NetBrain/log.txt 2>&1 &
