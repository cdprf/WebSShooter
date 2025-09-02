# ScreenShotter (WebSShooter)

WebSShooter is a .NET 6.0 console application that takes screenshots of web pages. It can process either a single URL or a file containing a list of URLs, and can optionally log results to a file.

## Features

- Takes screenshots of web pages using Selenium WebDriver with Chrome
- Processes single URLs or lists of URLs from a text file
- Supports multi-threaded processing for improved performance
- Configurable output directory for screenshots
- Optional logging to file with text or JSON format
- Automatic protocol fallback (HTTP to HTTPS and vice versa) for better compatibility
- Headless browser operation for efficient processing
- Customizable thread count for optimal performance
- Duplicate screenshot detection and removal

## Installation

1. Clone or download the repository
2. Ensure you have .NET 6.0 SDK installed
3. Build the project with `dotnet build`
4. Run the application with `dotnet run` or execute the compiled binary

## Cross-Platform Builds

WebSShooter can be built as a single-file executable for Windows, Linux, and macOS platforms. The project includes platform-specific build scripts that generate self-contained executables with all required dependencies.

### Windows Build

To build a Windows executable, run the batch script:
```cmd
publish-single-file.bat
```

This will create a single executable file at:
`bin\Release\net6.0\win-x64\publish\WebSShooter.exe`

### Linux and macOS Builds

To build executables for Linux and macOS, run the shell script:
```bash
./publish-single-file.sh
```

This will create single executable files at:
- Linux: `bin/Release/net6.0/linux-x64/publish/WebSShooter`
- macOS: `bin/Release/net6.0/osx-x64/publish/WebSShooter`

The script automatically sets execute permissions for the output files.

### Manual Build Commands

You can also build for specific platforms using the .NET CLI directly:

#### Windows (win-x64)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### Linux (linux-x64)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### macOS (osx-x64)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### Build Output Structure

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

### Running the Executables

After building, you can run the executables directly without requiring the .NET runtime to be installed:

#### Windows
```cmd
bin\Release\net6.0\win-x64\publish\WebSShooter.exe <url or url_list_file> [options]
```

#### Linux
```bash
./bin/Release/net6.0/linux-x64/publish/WebSShooter <url or url_list_file> [options]
```

#### macOS
```bash
./bin/Release/net6.0/osx-x64/publish/WebSShooter <url or url_list_file> [options]
```

## Usage

```bash
WebSShooter <url or url_list_file> [--threads <count>] [--out <folder>] [--log <logfile>] [--log-format <text|json>] [--remove-duplicates]
```

### Parameters

- `<url or url_list_file>`: A single URL or a path to a text file containing URLs (one per line)
- `--threads <count>` or `-t <count>`: Number of threads to use for processing URLs (default: 10)
- `--out <folder>` or `-o <folder>`: Output folder for screenshots (default: `./screenshots`)
- `--log <logfile>` or `-l <logfile>`: Log file path (default: no log file)
- `--log-format <text|json>`: Log format (default: text)
- `--remove-duplicates` or `-rd`: Remove duplicate screenshots after processing (default: false)
- `--help` or `-h`: Display help information

### Examples

```bash
# Process a single URL
WebSShooter http://example.com

# Process URLs from a file with default settings (10 threads)
WebSShooter urllist.txt

# Process URLs with custom thread count
WebSShooter urllist.txt --threads 20

# Process URLs with custom output directory
WebSShooter urllist.txt --out ./my_screenshots

# Process URLs with logging
WebSShooter urllist.txt --log results.log

# Process URLs with JSON logging format
WebSShooter urllist.txt --log results.log --log-format json

# Process URLs with custom thread count, output directory, and logging
WebSShooter urllist.txt --threads 15 --out ./my_screenshots --log processing.log

# Process URLs and remove duplicate screenshots
WebSShooter urllist.txt --remove-duplicates

# Display help information
WebSShooter --help
```

## Duplicate Screenshot Removal

The application can automatically detect and remove duplicate screenshots after processing is complete. This feature compares the content of all PNG files in the output directory and removes any duplicates, keeping only the first occurrence.

To enable this feature, use the `--remove-duplicates` or `-rd` flag:

```bash
WebSShooter urllist.txt --remove-duplicates
```

The duplicate removal process works by:
1. Calculating an MD5 hash for each PNG file in the output directory
2. Grouping files with identical hashes
3. Keeping the first file in each group and deleting the rest

This can significantly reduce disk space usage when processing websites that return identical content for multiple URLs.

## Multi-threading

The application supports multi-threaded processing of URLs for improved performance when processing large lists. By default, it uses 10 threads, but this can be customized with the `--threads` parameter.

Each thread creates its own WebDriver instance to ensure thread safety. URLs are distributed evenly among threads for balanced workload.

To maintain backward compatibility, you can set `--threads 1` to process URLs sequentially.

## Logging

WebSShooter supports two logging formats:

### Text Format (Default)
```
[2023-05-15 14:30:25] success: http://example.com -> C:/path/to/screenshots/screenshot_http___example.com.png
[2023-05-15 14:30:26] error: http://nonexistent.com | The remote name could not be resolved: 'nonexistent.com'
```

### JSON Format
```json
{"time":"2023-05-15T14:30:25","type":"success","url":"http://example.com","file":"C:/path/to/screenshots/screenshot_http___example.com.png","message":null}
{"time":"2023-05-15T14:30:26","type":"error","url":"http://nonexistent.com","file":null,"message":"The remote name could not be resolved: 'nonexistent.com'"}
```

## File Naming Convention

Screenshot files are named using the following pattern:
```
screenshot_<url>.png
```

Invalid characters in URLs are removed to create valid filenames. If the resulting filename exceeds 100 characters, it is truncated.

## Protocol Fallback

The application automatically attempts to fallback between HTTP and HTTPS protocols if a "Bad Request" response is received. This helps with websites that have moved from one protocol to another.

## License

This project is licensed under the GNU General Public License v3.0 - see [LICENSE.txt](LICENSE.txt) for details.