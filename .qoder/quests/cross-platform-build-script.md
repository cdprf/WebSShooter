# Cross-Platform Build Solution for ScreenShotter

## Overview

This document outlines a cross-platform build solution for the ScreenShotter application that generates single-file executable outputs for Windows, Linux, and macOS platforms. Due to file system restrictions, the solution is documented here with the commands needed to build for each platform, which can be executed manually or incorporated into build scripts by the user.

## Architecture

The solution leverages the .NET 6.0 SDK's cross-platform capabilities to generate single-file executables for each target OS. The build process uses the .NET CLI to create self-contained, single-file deployments for each platform.

### Key Components

1. **Windows Batch Script** - Builds Windows executable
2. **Shell Script** - Builds Linux and macOS executables
3. **Build Targets** - Windows, Linux, and macOS
4. **Single-File Output** - Each build produces a single executable file
5. **Self-Contained Deployment** - All builds include the .NET runtime

## Supported Platforms

The solution supports building for the following platforms:
- Windows (win-x64)
- Linux (linux-x64)
- macOS (osx-x64)

### Build Configuration

Each build uses the following configuration:
- Release configuration
- Self-contained deployment
- Single-file output
- Native library inclusion for self-extraction

### Output Structure

The builds are organized in the following directory structure:
```
bin/
├── Release/
│   ├── net6.0/
│   │   ├── win-x64/
│   │   │   └── publish/
│   │   │       └── WebSShooter.exe
│   │   ├── linux-x64/
│   │   │   └── publish/
│   │   │       └── WebSShooter
│   │   └── osx-x64/
│   │       └── publish/
│   │           └── WebSShooter
```

## Implementation Plan

### Manual Build Commands

Due to file system restrictions, users should execute the following commands manually or incorporate them into their own build scripts.

#### Windows Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The resulting executable will be located at:
```
bin/Release/net6.0/win-x64/publish/WebSShooter.exe
```

#### Linux Build
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The resulting executable will be located at:
```
bin/Release/net6.0/linux-x64/publish/WebSShooter
```

After building, set execute permissions:
```bash
chmod +x bin/Release/net6.0/linux-x64/publish/WebSShooter
```

#### macOS Build
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The resulting executable will be located at:
```
bin/Release/net6.0/osx-x64/publish/WebSShooter
```

After building, set execute permissions:
```bash
chmod +x bin/Release/net6.0/osx-x64/publish/WebSShooter
```

#### Building for All Platforms
To build for all platforms, execute each of the above commands in sequence:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
chmod +x bin/Release/net6.0/linux-x64/publish/WebSShooter

# macOS
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
chmod +x bin/Release/net6.0/osx-x64/publish/WebSShooter
```

### File Permissions

On Linux and macOS, the output executables need to have execute permissions set:
```bash
chmod +x bin/Release/net6.0/linux-x64/publish/WebSShooter
chmod +x bin/Release/net6.0/osx-x64/publish/WebSShooter
```

## Technical Considerations

### Runtime Dependencies

The application depends on:
- Selenium WebDriver (managed via WebDriverManager)
- ChromeDriver (downloaded automatically by WebDriverManager)

These dependencies are handled automatically by the application at runtime, so no special considerations are needed for the build process.

### Path Considerations

The build outputs use platform-appropriate path separators and conventions to ensure compatibility across different host operating systems.

## Testing Strategy

### Build Verification

1. Execute build commands on development machine
2. Verify all three platform builds complete successfully
3. Validate output file structure
4. Test executables on target platforms
5. Verify single-file nature of outputs

### Platform Testing

Each built executable should be tested on its target platform to ensure:
- Application starts correctly
- All command-line parameters work
- Screenshot functionality operates as expected
- Logging functionality works
- Multi-threading capabilities function

## Deployment

### Distribution Structure

The final distribution includes:
- Windows executable (WebSShooter.exe)
- Linux executable (WebSShooter)
- macOS executable (WebSShooter)
- README with usage instructions
- Sample urllist.txt file

### Release Process

1. Execute build commands for target platforms
2. Package outputs with appropriate naming conventions
3. Create release notes
4. Publish to distribution channel

## Security Considerations

### Code Signing

Consider implementing code signing for Windows executables to prevent security warnings.

### Antivirus False Positives

Single-file executables that include web automation libraries may trigger antivirus false positives. Include documentation to help users handle these situations.

## Performance Considerations

### Build Time

Building for all platforms may take considerable time. Consider building for one platform at a time on resource-constrained systems.

### Output Size

Single-file executables are larger than regular deployments due to the inclusion of the .NET runtime. Typical sizes:
- Windows: ~60-80 MB
- Linux: ~60-80 MB
- macOS: ~60-80 MB

## Troubleshooting

### Build Failures

If you encounter build failures:

1. Ensure you have the .NET 6.0 SDK installed
2. Check that you're running the commands from the project directory
3. Verify that you have sufficient disk space (builds can require several GB of temporary space)

### Runtime Issues

If the built executables fail to run:

1. Ensure Google Chrome is installed on the target system
2. Check that the target system meets the minimum requirements
3. On Linux/macOS, verify that execute permissions are set correctly

### Antivirus False Positives

Single-file executables that include web automation libraries may trigger antivirus false positives. This is a known issue with packaged .NET applications that use web automation. To address this:

1. Add the executable to your antivirus exclusions
2. Verify the executable with a hash check against the original build
3. Consider code signing for production distributions

## Future Enhancements

### Additional Platforms

The build approach allows for easy addition of other platforms:
- ARM architectures (win-arm64, linux-arm64)
- 32-bit versions (win-x86)

### Configuration Options

Future enhancements could include:
- Compression level settings
- Custom application icon for each platform
- Version embedding
- Build timestamp inclusion

## Cleaning Previous Builds

To clean previous builds before building, delete the `bin` directory:

```bash
rm -rf bin
```

On Windows:
```cmd
rmdir /s /q bin
```