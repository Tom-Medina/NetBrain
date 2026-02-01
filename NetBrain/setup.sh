#!/bin/bash
APP_DIR=~/NetBrain/repo/publish
USB_DIR=/mnt/usb

sudo mkdir -p $USB_DIR
sudo mount /dev/sda1 $USB_DIR

cp $USB_DIR/appsettings.json $APP_DIR/appsettings.json

sudo umount $USB_DIR
echo "appsettings.json copied to $APP_DIR"
