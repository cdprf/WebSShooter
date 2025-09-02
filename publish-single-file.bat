@echo off
echo Publishing WebSShooter as a single-file executable for Windows...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
echo.
echo Publishing complete!
echo The single-file executable is located at: bin\Release\net6.0\win-x64\publish\WebSShooter.exe
echo.
echo To run the application, execute:
echo bin\Release\net6.0\win-x64\publish\WebSShooter.exe
pause