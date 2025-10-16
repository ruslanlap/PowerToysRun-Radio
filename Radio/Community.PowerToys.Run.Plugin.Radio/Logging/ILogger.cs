using System;

namespace Community.PowerToys.Run.Plugin.Radio.Logging
{
    /// <summary>
    /// Logger interface for the Radio plugin.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message with optional exception.
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Logs a debug message (only in DEBUG builds).
        /// </summary>
        void LogDebug(string message);
    }
}
