using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Drawing.Imaging;
using System.IO;

void ShowHelp()
{
    Console.WriteLine("USAGE:");
    Console.WriteLine("  WebSShooter <url or url_list_file> [--out <folder>] [--log <logfile>] [--log-format <text|json>]");
    Console.WriteLine();
    Console.WriteLine("Description:");
    Console.WriteLine("  Provide a single URL or a file containing URLs (one per line). A screenshot will be taken for each URL.");
    Console.WriteLine("  Optionally, specify an output folder for screenshots with --out or -o. Default is './screenshots'.");
    Console.WriteLine("  Optionally, specify a log file with --log or -l. Default: no log file.");
    Console.WriteLine("  Optionally, specify log format with --log-format <text|json>. Default: text.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  WebSShooter http://example.com");
    Console.WriteLine("  WebSShooter urllist.txt --out C:/myshots");
    Console.WriteLine("  WebSShooter urllist.txt --log result.log");
    Console.WriteLine("  WebSShooter urllist.txt --log-format json");
    Console.WriteLine();
    Console.WriteLine("If no parameter is given or --help/-h is used, this help screen will be shown.");
}

string[] urls;
string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
string? logFile = null;
string logFormat = "text";

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
    }
}

if (!Directory.Exists(outputDir))
    Directory.CreateDirectory(outputDir);

StreamWriter? logWriter = null;
if (!string.IsNullOrEmpty(logFile))
    logWriter = new StreamWriter(logFile, append: true);

void Log(string type, string url, string? filePath, string? message)
{
    if (logFormat == "json")
    {
        var logObj = new {
            time = DateTime.Now.ToString("s"),
            type,
            url,
            file = filePath,
            message
        };
        string json = System.Text.Json.JsonSerializer.Serialize(logObj);
        Console.WriteLine(json);
        logWriter?.WriteLine(json);
    }
    else
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {url}";
        if (!string.IsNullOrEmpty(filePath))
            line += $" -> {filePath}";
        if (!string.IsNullOrEmpty(message))
            line += $" | {message}";
        Console.WriteLine(line);
        logWriter?.WriteLine(line);
    }
    logWriter?.Flush();
}

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
            string summary = ex.Message.Split('\n', '\r')[0];
            if (summary.Length > 100) summary = summary.Substring(0, 100) + "...";
            Log("error", url, null, summary);
        }
    }
}

logWriter?.Dispose();

string ConvertUrlToFileName(string url)
{
    char[] invalidChars = Path.GetInvalidFileNameChars();
    string fileName = new string(url.Where(ch => !invalidChars.Contains(ch)).ToArray());
    if (fileName.Length > 100)
    {
        fileName = fileName.Substring(0, 100);
    }
    return fileName;
}