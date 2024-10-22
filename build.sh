#! /bin/sh

if [ -d "bin" ]; then
    rm -rf bin
fi

if [ -d "obj" ]; then
    rm -rf obj
fi

dotnet publish

if [ "$1" = "-i" ] && [ -d "$HOME/cbin" ]; then
    if [ -f "$HOME/cbin/get" ]; then
        rm "$HOME/cbin/get"
    fi
    mv "bin/Release/net8.0/osx-arm64/publish/get" "$HOME/cbin/get"
fi

rm -rf bin/Release
rm -rf bin/Debug