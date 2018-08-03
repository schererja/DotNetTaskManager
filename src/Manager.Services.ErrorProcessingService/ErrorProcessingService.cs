using System;
using System.IO;
using System.Threading;
using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace Manager.Services.ErrorProcessingService
{
    /// <inheritdoc />
    /// <summary>
    ///     Error Processing Service to get information about errors
    /// </summary>
    public class ErrorProcessingService : IErrorProcessingService
    {
        /// <summary>
        ///     String used for the service name
        /// </summary>
        private const string SERVICE_NAME = "Error Processing Service";

        /// <summary>
        ///     String used for the memory location letting know if started or not.
        /// </summary>
        private const string SERVICE_MEMORY = "ErrorProcessingServiceStarted";

        /// <summary>
        ///     String of the default pipe name for this service
        /// </summary>
        private const string PIPE_NAME = "ERROR";

        /// <summary>
        ///     ILoggerService used for logging with NLog
        /// </summary>
        private readonly ILoggerService _loggerService;

        /// <summary>
        ///     IMemoryCache used to store things into memory
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     Pipe Client used to get information from the core
        /// </summary>
        private PipeClient _pipeClient;

        /// <summary>
        ///     Constructor for the Error Processing Service
        /// </summary>
        /// <param name="loggerService">
        ///     Requires an <see cref="ILoggerService" />
        /// </param>
        /// <param name="memoryCache"></param>
        public ErrorProcessingService(ILoggerService loggerService, IMemoryCache memoryCache)
        {
            _loggerService = loggerService;
            _memoryCache = memoryCache;
            _pipeClient = new PipeClient(PIPE_NAME);
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     A TaskModel of the task that needs to be processed
        /// </summary>
        private TaskModel TaskToProcess { get; set; }


        /// <summary>
        ///     Starts up the service
        /// </summary>
        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY)) _memoryCache.Set(SERVICE_MEMORY, true);
            while ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _pipeClient.CreatePipe();
                TaskToProcess = new TaskModel(_pipeClient.ReadString());
                ProcessTask();
                _pipeClient.WaitForDrain();
                _pipeClient = new PipeClient(PIPE_NAME);
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        ///     Stops the service
        /// </summary>
        public void Stop()
        {
            try
            {
                // Wait for Pipe communication to stop
                _pipeClient.WaitForDrain();

                // Close Pipe
                _pipeClient.Dispose();

                _memoryCache.Set(SERVICE_MEMORY, false);
            }

            catch (ObjectDisposedException ode)
            {
                _loggerService.LogMessage(SERVICE_NAME,
                    ode.Message, LogLevel.Error);

                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new ErrorProcessingServiceException(ode.Message);
            }

            catch (NotSupportedException nse)
            {
                _loggerService.LogMessage(SERVICE_NAME,
                    nse.Message, LogLevel.Error);

                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new ErrorProcessingServiceException(nse.Message);
            }

            catch (IOException ioe)
            {
                _loggerService.LogMessage(SERVICE_NAME,
                    ioe.Message, LogLevel.Error);

                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new ErrorProcessingServiceException(ioe.Message);
            }

            catch (InvalidOperationException ioe)
            {
                _loggerService.LogMessage(SERVICE_NAME,
                    ioe.Message, LogLevel.Error);

                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new ErrorProcessingServiceException(ioe.Message);
            }
        }

        /// <summary>
        ///     Restarts the service
        /// </summary>
        public void Restart()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                Start();
            }
            else if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                Stop();
                Start();
            }
        }

        /// <summary>
        ///     Used to process the task
        /// </summary>
        public void ProcessTask()
        {
            var errorsFound = TaskToProcess.Messages.Where(m => m.Category == Category.Error);
            foreach (var errorFound in errorsFound)
                _loggerService.LogMessage(SERVICE_NAME, errorFound.Message, LogLevel.Error);
        }
    }
}