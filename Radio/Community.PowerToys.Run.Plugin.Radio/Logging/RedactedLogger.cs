using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Community.PowerToys.Run.Plugin.Radio.Logging
{
    /// <summary>
    /// Logger implementation with PII redaction.
    /// </summary>
    public sealed class RedactedLogger : ILogger
    {
        private readonly string _logPath;
        private readonly object _lock = new object();

        public RedactedLogger(string pluginName)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDir = Path.Combine(appData, "Microsoft", "PowerToys", "PowerToys Run", "Logs");

            try
            {
                Directory.CreateDirectory(logDir);
                _logPath = Path.Combine(logDir, $"{pluginName}.log");
            }
            catch
            {
                // Fallback to temp directory if can't create in LocalAppData
                _logPath = Path.Combine(Path.GetTempPath(), $"{pluginName}.log");
            }
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARN", message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            var fullMessage = exception != null
                ? $"{message}: {exception.Message}\n{exception.StackTrace}"
                : message;
            Log("ERROR", Redact(fullMessage));
        }

        public void LogDebug(string message)
        {
#if DEBUG
            Log("DEBUG", message);
#endif
        }

        private void Log(string level, string message)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}\n";

                lock (_lock)
                {
                    File.AppendAllText(_logPath, logEntry);
                }
            }
            catch
            {
                // Silently fail - don't crash plugin if logging fails
            }
        }

        private static string Redact(string message)
        {
            // Redact IP addresses
            message = Regex.Replace(message, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "[IP_REDACTED]");

            // Redact email addresses
            message = Regex.Replace(message, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", "[EMAIL_REDACTED]");

            // Redact passwords, tokens, keys in URLs
            message = Regex.Replace(message, @"(password|token|key|apikey)=[^&\s]+", "$1=[REDACTED]", RegexOptions.IgnoreCase);

            return message;
        }
    }
}
