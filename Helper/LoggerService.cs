using System;
using System.IO;
using Serilog;

namespace AccuBooks_ERP.Helper
{
    public static class LoggerService
    {
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AccuBooksERP",
                "Logs"
            );

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            string logFilePath = Path.Combine(logDir, "accubooks-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            _isInitialized = true;
            Log.Information("AccuBooks ERP Application Started");
        }

        public static void Info(string message) => Log.Information(message);
        public static void Warning(string message) => Log.Warning(message);
        public static void Error(Exception ex, string message) => Log.Error(ex, message);
        public static void Error(string message) => Log.Error(message);
        public static void CloseAndFlush() => Log.CloseAndFlush();
    }
}
