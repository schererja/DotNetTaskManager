using System;
using System.IO;
using System.Text;
using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using static System.Environment;

namespace Manager.Services.PipeService
{
    public class PipeService : IPipeService
    {
        /// <summary>
        ///     String of the Service Name
        /// </summary>
        private const string SERVICE_NAME = "Pipe Service";

        /// <summary>
        ///     String of the service memory used if pipe service is turned on
        /// </summary>
        private const string SERVICE_MEMORY = "PipeServiceStarted";

        /// <summary>
        ///     String  for the Default Pipe Name
        /// </summary>
        private const string PIPE_NAME = "CORE";

        /// <summary>
        ///     ILoggerService used for the logger
        /// </summary>
        private readonly ILoggerService _logger;

        /// <summary>
        ///     IMemoryCache used for talking to memory and storing information
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     PipeServer used for communication with core
        /// </summary>
        private PipeServer _pipeServer;


        /// <summary>
        ///     Constructor for PipeService
        /// </summary>
        /// <param name="memoryCache">Requires a <see cref="IMemoryCache" /></param>
        /// <param name="logger">Requires a <see cref="ILoggerService" /></param>
        public PipeService(IMemoryCache memoryCache,
            ILoggerService logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     Used to Restart the Service
        /// </summary>
        public void Restart()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Starts a Pipe Server in this Pipe Service
        /// </summary>
        public void Start()
        {
            using (_pipeServer = new PipeServer())
            {
                try
                {
                    _memoryCache.Set(SERVICE_MEMORY, true);
                    _logger.LogMessage(SERVICE_NAME, "Started", LogLevel.Info);

                    while ((bool) _memoryCache.Get(SERVICE_MEMORY))
                    {
                        // Potential request from client

                        try
                        {
                            // Create a new Pipe for connection to this Pipe Server. Waits
                            // for a client connection
                            _pipeServer.CreatePipe();
                            _logger.LogMessage(SERVICE_NAME, "New Pipe Created", LogLevel.Info);

                            // Request from client received
                            var request = _pipeServer.ReadString();

                            // Process request from client
                            ProcessRequest(request);
                            // Sent response to client
                            _pipeServer.WriteString("Accepted Task");

                            // Wait for all Pipe communication to stop
                            _pipeServer.WaitForDrain();
                        }

                        catch (NotSupportedException nse)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);
                        }

                        catch (ObjectDisposedException ode)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, ode.Message, LogLevel.Error);
                        }

                        catch (ArgumentNullException ane)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);
                        }

                        catch (ArgumentOutOfRangeException aoore)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, aoore.Message, LogLevel.Error);
                        }

                        catch (IOException ioe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            Start();
                            _logger.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);
                        }

                        catch (DecoderFallbackException dfe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, dfe.Message, LogLevel.Error);

                            // _logger.LogError(dfe.Message);
                        }

                        catch (ArgumentException ae)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);

                            // _logger.LogError(ae.Message);
                        }

                        catch (OverflowException oe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _logger.LogMessage(SERVICE_NAME, oe.Message, LogLevel.Error);

                            // _logger.LogError(oe.Message);
                        }

                        _pipeServer.Disconnect();
                        _pipeServer = new PipeServer();
                    }
                }

                catch (ArgumentNullException ane)
                {
                    //_logger.LogError(ane.Message);
                    _logger.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);


                    throw new PipeServiceException(ane.Message);
                }

                catch (ArgumentOutOfRangeException aoore)
                {
                    //_logger.LogError(aoore.Message);
                    _logger.LogMessage(SERVICE_NAME, aoore.Message, LogLevel.Error);

                    throw new PipeServiceException(aoore.Message);
                }

                catch (ArgumentException ae)
                {
                    //_logger.LogError(ae.Message);
                    _logger.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);

                    throw new PipeServiceException(ae.Message);
                }

                catch (PlatformNotSupportedException pnse)
                {
                    _logger.LogMessage(SERVICE_NAME, pnse.Message, LogLevel.Error);

                    throw new PipeServiceException(pnse.Message);
                }

                catch (NotSupportedException nse)
                {
                    _logger.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);

                    throw new PipeServiceException(nse.Message);
                }

                catch (IOException ioe)
                {
                    _logger.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);

                    throw new PipeServiceException(ioe.Message);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Stops a Pipe Server with this Pipe Service. Waits for all
        ///     Pipe Communication to halt, then closes all Pipe(s)
        /// </summary>
        public void Stop()
        {
            try
            {
                // Wait for Pipe communication to stop
                _pipeServer.WaitForDrain();

                _pipeServer.Disconnect();

                // Close Pipe
                _pipeServer.Dispose();

                _memoryCache.Set(SERVICE_MEMORY, false);
            }

            catch (ObjectDisposedException ode)
            {
                _logger.LogMessage(SERVICE_NAME, ode.Message, LogLevel.Error);


                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new PipeServiceException(ode.Message);
            }

            catch (NotSupportedException nse)
            {
                _logger.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);


                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new PipeServiceException(nse.Message);
            }

            catch (IOException ioe)
            {
                _logger.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);


                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new PipeServiceException(ioe.Message);
            }

            catch (InvalidOperationException ioe)
            {
                _logger.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);


                _memoryCache.Set(SERVICE_MEMORY, false);

                throw new PipeServiceException(ioe.Message);
            }
        }


        /// <summary>
        ///     Processes a request (Task) from a Client Pipe.
        ///     Creates a Task from request's string via TaskModel (if possible).
        ///     Adds a successfully created Task to the Task Queue for further
        ///     processing
        /// </summary>
        /// <param name="request">
        ///     The Client Pipe's request (Task) as a string
        /// </param>
        private void ProcessRequest(string request)
        {
            _logger.LogMessage(SERVICE_NAME, "Processing: " + request + "\n", LogLevel.Info);
            if (OSVersion.Platform == PlatformID.Win32NT) request = request.Replace("\\", "\\\\");
            //_logger.LogMessage(SERVICE_NAME, "Modified Request:" + request, LogLevel.Info);
            // Create a Task Model from request's Task string
            var task = new TaskModel(request);
            _logger.LogMessage(SERVICE_NAME, "Processing: " + task.Name, LogLevel.Info);
            // Adds new Task Model to Task Queue
            var tempTaskQueue = (TaskQueue) _memoryCache.Get("taskQueue");
            if (task.WhereFrom == "Unknown" ||
                task.WhereTo == "Unknown")
                _logger.LogMessage(SERVICE_NAME, task.Name, LogLevel.Error);
            else
                _logger.LogMessage(SERVICE_NAME, task.Name + " Added to Queue", LogLevel.Info);
            tempTaskQueue.Queue.Add(task);
            _memoryCache.Set("taskQueue", tempTaskQueue);
        }
    }
}