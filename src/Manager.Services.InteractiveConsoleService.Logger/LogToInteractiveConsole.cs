using System;
using Manager.LIBS.Pipe;
using Manager.Services.InteractiveConsoleService.DrawScreen;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Manager.Services.InteractiveConsoleService.Logger
{
    /// <inheritdoc />
    /// <summary>
    ///     Class abstraction of Interactive CLI Logger for the top
    ///     sub-window of the Console window screen. Implements the
    ///     <see cref="T:Core.Service.InteractiveConsole.Logger.ILogToInteractiveConsole" />
    ///     interface for dependency injection in other sub-projects
    /// </summary>
    public class LogToInteractiveConsole : ILogToInteractiveConsole
    {
        private const string SERVICE_NAME = "Interactive Console Logger";
        private const string SERVICE_MEMORY = "InteractiveConsoleLoggerStarted";

        private const string PIPE_NAME = "InteractiveLogger";

        // Draw Screen Service dependency injection
        private readonly IDrawScreen _drawScreen;

        // Memory Target for log in memory
        private readonly MemoryTarget _target;

        /// <summary>
        ///     Constructor for Interactive CLI
        ///     Logger. Sets the Draw Screen Service and in memory location of the
        ///     log
        /// </summary>
        /// <param name="drawScreen">The Draw Screen Service to set</param>
        public LogToInteractiveConsole(IDrawScreen drawScreen)
        {
            _drawScreen = drawScreen;
            _target = LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory");
            PipeServer = new PipeServer(PIPE_NAME);
        }

        public PipeServer PipeServer { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Logs all output to the
        ///     Interactive Console as a string
        /// </summary>
        /// <param name="message">
        ///     The string message to log to the Interactive CLI
        /// </param>
        /// <param name="logLevel">
        ///     The NLog log level (i.e. Trace, Debug, Info, Warn, Error, or Fatal
        /// </param>
        public void LogOutputToInteractiveConsole(string message,
            LogLevel logLevel)
        {
            // Length of incoming log message
            var length = message.Length;

            var consoleWidth = Console.WindowWidth - 36;

            // Split incoming log message based on Console Window Width for
            // more appropriate log message displaying
            var spaceDelim = Math.Ceiling(length /
                                          (double) consoleWidth);
            SimpleConfigurator.ConfigureForTargetLogging(_target,
                logLevel);

            // Obtain the log in memory
            var logger = LogManager.GetLogger("logMemory");

            // Incoming log message length is greater than Console Window Width
            if (spaceDelim > 1)
            {
                // Beginning sub-string character position
                var beginSubStr = 0;

                // Loop for delimeter times
                for (var i = 1; i <= spaceDelim; i++)
                {
                    var temp = "";
                    if (consoleWidth * i > length)
                    {
                        temp = message.Substring(beginSubStr,
                            length - beginSubStr);
                    }
                    else
                    {
                        // Split message dynamically
                        temp = message.Substring(beginSubStr,
                            consoleWidth);
                        beginSubStr = temp.Length * i;
                    }

                    // Log message to Interactive CLI at NLog level
                    switch (logLevel.ToString())
                    {
                        case "Trace":

                            logger.Trace(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        case "Debug":

                            logger.Debug(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        case "Info":

                            logger.Info(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        case "Warn":

                            logger.Warn(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        case "Error":

                            logger.Error(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        case "Fatal":

                            logger.Fatal(temp);
                            // Draw log message on Interactive CLI top sub-window
                            _drawScreen.DrawLogScreenNow();

                            break;

                        default:

                            break;
                    }
                }
            }

            // Incoming log message length is within than Console Window Width
            else
            {
                switch (logLevel.ToString())
                {
                    case "Trace":

                        logger.Trace(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    case "Debug":

                        logger.Debug(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    case "Info":

                        logger.Info(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    case "Warn":

                        logger.Warn(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    case "Error":

                        logger.Error(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    case "Fatal":

                        logger.Fatal(message);
                        // Draw log message on Interactive CLI top sub-window
                        _drawScreen.DrawLogScreenNow();

                        break;

                    default:

                        break;
                }
            }
        }
    }
}