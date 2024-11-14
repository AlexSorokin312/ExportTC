using Serilog;

public static class AppLogger
{
    static AppLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void LogInformation(string message)
    {
        Log.Information(message);
    }

    public static void LogError(Exception exception, string message)
    {
        Log.Error(exception, message);
    }

    public static void LogFatal(Exception exception, string message)
    {
        Log.Fatal(exception, message);
    }

    public static void CloseLogger()
    {
        Log.CloseAndFlush();
    }
}