using System.Threading;
using Manager.Services.InteractiveConsoleService.Logger;
using Manager.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using LogLevel = NLog.LogLevel;

namespace Manager.Services.LoggerService
{
    /// <summary>
    ///     Logger service, allows for writing logs to both interactive console and console
    /// </summary>
    public class LoggerService : ILoggerService
    {
        private const string SERVICE_NAME = "Logger Service";
        private const string SERVICE_MEMORY = "LoggerServiceStarted";
        private const string INTERACTIVE_SERVICE = "InteractiveConsoleService";
        private readonly AppSettings _config;
        private readonly ILogToInteractiveConsole _interactiveConsole;
        private readonly ILogger<LoggerService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly Logger _nLogLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Constructor for the logger service
        /// </summary>
        /// <param name="interactiveConsole">
        ///     Requires an <see cref="ILogToInteractiveConsole" />
        /// </param>
        /// <param name="logger">
        ///     Requires an <see cref="Microsoft.Extensions.Logging.ILogger" />
        /// </param>
        /// <param name="config">
        ///     Requires an <see cref="IOptions{TOptions}" />
        /// </param>
        /// <param name="memoryCache"></param>
        public LoggerService(ILogToInteractiveConsole interactiveConsole,
            ILogger<LoggerService> logger,
            IOptions<AppSettings> config,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _interactiveConsole = interactiveConsole;
            _logger = logger;
            _config = config.Value;
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     Used to log a message, logs to where needed, either interactive console or normal console
        /// </summary>
        /// <param name="serviceName">Requires a <see cref="string" /></param>
        /// <param name="message">Requires a <see cref="string" /></param>
        /// <param name="logLevel">Requires a <see cref="NLog.LogLevel" /></param>
        public void LogMessage(string serviceName, string message, LogLevel logLevel)
        {
            var delimiter = ":";
            var space = " ";
            var finalLogMessage = serviceName + delimiter + space + message;
            var service = _config.Components.Find(comp => comp.Name == INTERACTIVE_SERVICE);
            switch (service.Name)
            {
                case INTERACTIVE_SERVICE when service.Properties.Enabled:
                    _nLogLogger.Log(logLevel, finalLogMessage);
                    _interactiveConsole.LogOutputToInteractiveConsole(finalLogMessage, logLevel);
                    break;
                case INTERACTIVE_SERVICE when !service.Properties.Enabled:
                    switch (logLevel.ToString())
                    {
                        case "Debug":

                            _logger.LogDebug(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        case "Error":
                            _logger.LogError(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        case "Fatal":
                            _logger.LogCritical(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        case "Info":
                            _logger.LogInformation(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        case "Off":
                            break;
                        case "Trace":
                            _logger.LogTrace(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        case "Warn":
                            _logger.LogWarning(finalLogMessage);
                            _nLogLogger.Log(logLevel, finalLogMessage);
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Used to start the Logger Service
        /// </summary>
        public void Start()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
            }
            else
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
        }

        /// <summary>
        ///     Used to stop the Logger Service
        /// </summary>
        public void Stop()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY)) _memoryCache.Set(SERVICE_MEMORY, false);
        }


        /// <summary>
        ///     Used to restart the Logger Service
        /// </summary>
        public void Restart()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, false);
                Thread.Sleep(500);
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
            else
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
        }
    }
}