using NLog;

namespace Manager.Services.LoggerService
{
    /// <summary>
    ///     Interface for the Logger Service
    /// </summary>
    public interface ILoggerService
    {
        void LogMessage(string serviceName, string message, LogLevel logLevel);
    }
}