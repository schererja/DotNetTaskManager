using NLog;

namespace Manager.Services.InteractiveConsoleService.Logger
{
    /// <summary>
    ///     Interface for the log to interactive console
    /// </summary>
    public interface ILogToInteractiveConsole
    {
        void LogOutputToInteractiveConsole(string message, LogLevel logLevel);
    }
}