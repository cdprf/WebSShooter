using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

class Program
{
    // Lock object for thread-safe logging
    private static readonly object logLock = new object();
    private static string? logFile = null;
    private static string logFormat = "text";
    private static string outputDir = "";
    private static StreamWriter? logWriter = null;

    static async Task Main(string[] args)
    {
        try
        {
            await RunApplication(args);
        }
        catch (Exception ex)
        {
            ConsoleWrite($"An unexpected error occurred: {ex.Message}", ConsoleColor.Red);
        }
    }

    static async Task RunApplication(string[] args)
    {
        try
        {
            string[] urls;
            outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
            logFile = null;
            logFormat = "text";
            int threadCount = 10; // Default thread count

            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return;
            }
            else
            {
                var input = args[0];
                if (File.Exists(input))
                {
                    urls = File.ReadAllLines(input);
                }
                else
                {
                    urls = new string[] { input };
                }
                // Output folder, log file, log format parse
                for (int i = 1; i < args.Length; i++)
                {
                    if ((args[i] == "--out" || args[i] == "-o") && i + 1 < args.Length)
                    {
                        outputDir = args[i + 1];
                        i++;
                    }
                    else if ((args[i] == "--log" || args[i] == "-l") && i + 1 < args.Length)
                    {
                        logFile = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "--log-format" && i + 1 < args.Length)
                    {
                        var fmt = args[i + 1].ToLower();
                        if (fmt == "json" || fmt == "text")
                            logFormat = fmt;
                        i++;
                    }
                    else if ((args[i] == "--threads" || args[i] == "-t") && i + 1 < args.Length)
                    {
                        if (int.TryParse(args[i + 1], out int threads))
                        {
                            threadCount = threads > 0 ? threads : 10;
                        }
                        i++;
                    }
                }
            }

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!string.IsNullOrEmpty(logFile))
                logWriter = new StreamWriter(logFile, append: true);

            // If only one thread is requested, process sequentially (backward compatibility)
            if (threadCount <= 1)
            {
                try
                {
                    new DriverManager().SetUpDriver(new ChromeConfig());

                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--headless");
                    options.AddArgument("--disable-gpu");
                    options.AddArgument("--window-size=1280,800");
                    options.AddArgument("--ignore-certificate-errors");
                    options.AddExcludedArgument("enable-automation");
                    options.AddExcludedArgument("load-extension");
                    options.AddArgument("--log-level=3");
                    options.AddArgument("--silent");
                    options.AddArgument("--disable-logging");

                    var service = ChromeDriverService.CreateDefaultService();
                    service.SuppressInitialDiagnosticInformation = true;
                    service.HideCommandPromptWindow = true;
                    service.EnableVerboseLogging = false;

                    using (IWebDriver driver = new ChromeDriver(service, options))
                    {
                        foreach (var url in urls)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl(url);
                                if (driver.PageSource.Contains("Bad Request") && (url.StartsWith("http://")))
                                    driver.Navigate().GoToUrl(url.Replace("http://", "https://"));
                                else if (driver.PageSource.Contains("Bad Request") && (url.StartsWith("https://")))
                                    driver.Navigate().GoToUrl(url.Replace("https://", "http://"));

                                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                                string fileName = $"screenshot_{ConvertUrlToFileName(url)}.png";
                                string filePath = Path.Combine(outputDir, fileName);
                                using (MemoryStream stream = new MemoryStream(screenshot.AsByteArray))
                                {
                                    using (var fileStream = File.Create(filePath))
                                    {
                                        stream.CopyTo(fileStream);
                                    }
                                }

                                Log("success", url, filePath, null);
                            }
                            catch (Exception ex)
                            {
                                string summary = ex.Message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                if (summary.Length > 100) summary = summary.Substring(0, 100) + "...";
                                Log("error", url, null, summary);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrite($"Error in single-threaded mode: {ex.Message}", ConsoleColor.Red);
                }
            }
            else
            {
                // Process URLs using multiple threads
                await ProcessUrlsWithThreads(urls, threadCount);
            }

            logWriter?.Dispose();
        }
        catch (Exception ex)
        {
            ConsoleWrite($"Error during application execution: {ex.Message}", ConsoleColor.Red);
        }
    }

    static void ShowHelp()
    {
        ConsoleWrite("USAGE:", ConsoleColor.Yellow);
        Console.WriteLine("  WebSShooter <url or url_list_file> [--threads <count>] [--out <folder>] [--log <logfile>] [--log-format <text|json>]");
        Console.WriteLine();
        ConsoleWrite("Description:", ConsoleColor.Yellow);
        Console.WriteLine("  Provide a single URL or a file containing URLs (one per line). A screenshot will be taken for each URL.");
        Console.WriteLine("  Optionally, specify an output folder for screenshots with --out or -o. Default is './screenshots'.");
        Console.WriteLine("  Optionally, specify a log file with --log or -l. Default: no log file.");
        Console.WriteLine("  Optionally, specify log format with --log-format <text|json>. Default: text.");
        Console.WriteLine("  Optionally, specify number of threads with --threads or -t. Default: 10.");
        Console.WriteLine();
        ConsoleWrite("Examples:", ConsoleColor.Yellow);
        Console.WriteLine("  WebSShooter http://example.com");
        Console.WriteLine("  WebSShooter urllist.txt --out C:/myshots");
        Console.WriteLine("  WebSShooter urllist.txt --log result.log");
        Console.WriteLine("  WebSShooter urllist.txt --log-format json");
        Console.WriteLine("  WebSShooter urllist.txt --threads 20");
        Console.WriteLine();
        Console.WriteLine("If no parameter is given or --help/-h is used, this help screen will be shown.");
    }

    static void Log(string type, string url, string? filePath, string? message)
    {
        lock (logLock)
        {
            ConsoleColor color = type == "success" ? ConsoleColor.Green : ConsoleColor.Red;
            if (logFormat == "json")
            {
                var logObj = new
                {
                    time = DateTime.Now.ToString("s"),
                    type,
                    url,
                    file = filePath,
                    message
                };
                string json = System.Text.Json.JsonSerializer.Serialize(logObj);
                ConsoleWrite(json, color);
                logWriter?.WriteLine(json);
            }
            else
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {url}";
                if (!string.IsNullOrEmpty(filePath))
                    line += $" -> {filePath}";
                if (!string.IsNullOrEmpty(message))
                    line += $" | {message}";
                ConsoleWrite(line, color);
                logWriter?.WriteLine(line);
            }
            logWriter?.Flush();
        }
    }

    // Partition URLs into chunks for thread distribution
    static List<string[]> PartitionUrls(string[] urls, int threadCount)
    {
        var partitions = new List<string[]>();
        int chunkSize = (int)Math.Ceiling((double)urls.Length / threadCount);

        for (int i = 0; i < urls.Length; i += chunkSize)
        {
            int remaining = urls.Length - i;
            int currentChunkSize = Math.Min(chunkSize, remaining);
            var chunk = new string[currentChunkSize];
            Array.Copy(urls, i, chunk, 0, currentChunkSize);
            partitions.Add(chunk);
        }

        // If we have fewer partitions than threads, that's fine
        return partitions;
    }

    // Process a chunk of URLs
    static async Task ProcessUrlChunk(string[] urlChunk, int threadId)
    {
        ConsoleWrite($"Thread {threadId} started, processing {urlChunk.Length} URLs.", ConsoleColor.Cyan);
        try
        {
            // Each thread needs its own WebDriver instance
            new DriverManager().SetUpDriver(new ChromeConfig());

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1280,800");
            options.AddArgument("--ignore-certificate-errors");
            options.AddExcludedArgument("enable-automation");
            options.AddExcludedArgument("load-extension");
            options.AddArgument("--log-level=3");
            options.AddArgument("--silent");
            options.AddArgument("--disable-logging");

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            service.EnableVerboseLogging = false;

            using (IWebDriver driver = new ChromeDriver(service, options))
            {
                foreach (var url in urlChunk)
                {
                    try
                    {
                        driver.Navigate().GoToUrl(url);
                        if (driver.PageSource.Contains("Bad Request") && (url.StartsWith("http://")))
                            driver.Navigate().GoToUrl(url.Replace("http://", "https://"));
                        else if (driver.PageSource.Contains("Bad Request") && (url.StartsWith("https://")))
                            driver.Navigate().GoToUrl(url.Replace("https://", "http://"));

                        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                        string fileName = $"screenshot_{ConvertUrlToFileName(url)}.png";
                        string filePath = Path.Combine(outputDir, fileName);
                        using (MemoryStream stream = new MemoryStream(screenshot.AsByteArray))
                        {
                            using (var fileStream = File.Create(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                        }

                        Log("success", url, filePath, null);
                    }
                    catch (Exception ex)
                    {
                        string summary = ex.Message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (summary.Length > 100) summary = summary.Substring(0, 100) + "...";
                        Log("error", url, null, summary);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleWrite($"Error in thread {threadId}: {ex.Message}", ConsoleColor.Red);
        }
        finally
        {
            ConsoleWrite($"Thread {threadId} completed.", ConsoleColor.Cyan);
        }
    }

    // Process URLs using multiple threads
    static async Task ProcessUrlsWithThreads(string[] urls, int threadCount)
    {
        ConsoleWrite($"Processing {urls.Length} URLs using {threadCount} threads...", ConsoleColor.Yellow);

        var urlPartitions = PartitionUrls(urls, threadCount);
        var tasks = new List<Task>();

        for (int i = 0; i < urlPartitions.Count; i++)
        {
            int threadId = i + 1;
            var partition = urlPartitions[i];
            tasks.Add(ProcessUrlChunk(partition, threadId));
        }

        await Task.WhenAll(tasks);
        ConsoleWrite("All threads completed.", ConsoleColor.Yellow);
    }

    static string ConvertUrlToFileName(string url)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string fileName = new string(url.Where(ch => !invalidChars.Contains(ch)).ToArray());
        if (fileName.Length > 100)
        {
            fileName = fileName.Substring(0, 100);
        }
        return fileName;
    }

    static void ConsoleWrite(string message, ConsoleColor color)
    {
        lock (logLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
