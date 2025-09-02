#!/bin/bash

echo "Publishing WebSShooter as a single-file executable for Linux and macOS..."

# Build for Linux
echo "Building for Linux (linux-x64)..."
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
echo "Linux build complete!"
echo "The Linux executable is located at: bin/Release/net6.0/linux-x64/publish/WebSShooter"

echo ""

# Build for macOS
echo "Building for macOS (osx-x64)..."
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
echo "macOS build complete!"
echo "The macOS executable is located at: bin/Release/net6.0/osx-x64/publish/WebSShooter"

echo ""

# Set execute permissions
echo "Setting execute permissions for Linux and macOS executables..."
chmod +x bin/Release/net6.0/linux-x64/publish/WebSShooter
chmod +x bin/Release/net6.0/osx-x64/publish/WebSShooter

echo ""
echo "All builds complete!"
echo "Executables:"
echo "  Linux: bin/Release/net6.0/linux-x64/publish/WebSShooter"
echo "  macOS: bin/Release/net6.0/osx-x64/publish/WebSShooter"
echo ""
echo "To run on Linux: ./bin/Release/net6.0/linux-x64/publish/WebSShooter"
echo "To run on macOS: ./bin/Release/net6.0/osx-x64/publish/WebSShooter"