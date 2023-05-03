#!/bin/sh
dotnet publish -c Release --runtime linux-arm64 --self-contained
rsync -av --delete /home/darklight/Projects/DarkHomebrewRadio/bin/Release/net6.0/linux-arm64/publish/ 10.0.20.10:DarkHomebrewRadio/ 
