using System;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;

public static class LoggerDebug
{
    private static readonly ILogger Logger;

    static LoggerDebug()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void LogMessage(
        string message,
        string logLevel = "INFO",
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        string className = ExtractClassName(filePath);

        var logEvent = $"{message} (Class: {className}, Method: {caller}, Line: {lineNumber})";

        switch (logLevel.ToUpper())
        {
            case "DEBUG":
                Logger.Debug(logEvent);
                break;
            case "INFO":
                Logger.Information(logEvent);
                break;
            case "WARNING":
                Logger.Warning(logEvent);
                break;
            case "ERROR":
                Logger.Error(logEvent);
                break;
            case "CRITICAL":
                Logger.Fatal(logEvent);
                break;
            default:
                Logger.Information(logEvent);
                break;
        }
    }

    private static string ExtractClassName(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return "UnknownClass";

            int lastSlashIndex = filePath.LastIndexOfAny(new[] { '\\', '/' });
            if (lastSlashIndex >= 0 && lastSlashIndex < filePath.Length - 1)
            {
                string fileName = filePath.Substring(lastSlashIndex + 1);
                return Path.GetFileNameWithoutExtension(fileName);
            }

            return Path.GetFileNameWithoutExtension(filePath);
        }
        catch
        {
            return "UnknownClass";
        }
    }

    public static void LogDebug(string message,
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogMessage(message, "DEBUG", caller, filePath, lineNumber);
    }

    public static void LogInfo(string message,
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogMessage(message, "INFO", caller, filePath, lineNumber);
    }

    public static void LogWarning(string message,
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogMessage(message, "WARNING", caller, filePath, lineNumber);
    }

    public static void LogError(string message,
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogMessage(message, "ERROR", caller, filePath, lineNumber);
    }

    public static void LogCritical(string message,
        [CallerMemberName] string caller = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        LogMessage(message, "CRITICAL", caller, filePath, lineNumber);
    }
}
