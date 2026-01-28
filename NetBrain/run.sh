#!/bin/bash
USB_PATH=/mnt/usb
APP_DIR=~/NetBrain
USB_DEV=/dev/sda1
EXE=NetBrain
mount | grep $USB_PATH >/dev/null || sudo mount $USB_DEV $USB_PATH

mkdir -p $APP_DIR
cp -r $USB_PATH/* $APP_DIR/
sudo umount $USB_PATH
cd $APP_DIR/publish
chmod +x $EXE
./$EXE
 