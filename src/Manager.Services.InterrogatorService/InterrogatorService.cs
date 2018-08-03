using Manager.LIBS.Model;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;

namespace Manager.Services.InterrogatorService
{
    public class InterrogatorService : IInterrogatorService
    {
        /// <summary>
        ///     A String of the Service Name
        /// </summary>
        private const string SERVICE_NAME = "Interrogator Service";

        /// <summary>
        ///     A String of the Service Memory location for knwoing if service should be on or off
        /// </summary>
        private const string SERVICE_MEMORY = "InterrogatorServiceStarted";

        /// <summary>
        ///     A String of the default pipe name for the service
        /// </summary>
        private const string PIPE_NAME = "INTERROGATOR";

        /// <summary>
        ///     A Task Model to be used for the Interrogation Service
        /// </summary>
        private static TaskModel _task;

        /// <summary>
        ///     Used as the logger for the service
        /// </summary>
        private readonly ILoggerService _logger;

        /// <summary>
        ///     Used to access the memory
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     Pipe client used for getting the data
        /// </summary>
        private PipeClient _pipeClient;

        /// <summary>
        ///     Constructor for the Interrogator Service
        /// </summary>
        /// <param name="logger">
        ///     Requires an ILoggerService as the logger
        /// </param>
        /// <param name="memoryCache">
        ///     Requires an IMemoryCache for communicating with the memory
        /// </param>
        public InterrogatorService(ILoggerService logger,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _pipeClient = new PipeClient(PIPE_NAME);

            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     Used as a way to start the service
        /// </summary>
        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY)) _memoryCache.Set(SERVICE_MEMORY, true);

            while ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _pipeClient.CreatePipe();

                var taskReceived = _pipeClient.ReadString();
                _task = new TaskModel(taskReceived);
                ProcessTask();
            }
        }

        /// <summary>
        ///     Used as a way to stop the service
        /// </summary>
        public void Stop()
        {
            _memoryCache.Set(SERVICE_MEMORY, false);
        }

        /// <summary>
        ///     Used as a way to restart the service
        /// </summary>
        public void Restart()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
            else
            {
                Stop();
                Start();
            }
        }

        /// <summary>
        ///     Used to process the task
        /// </summary>
        private void ProcessTask()
        {
            MessageModel messageToAdd;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                _task.Messages.Count(m => m.Category == Category.Start) <= 0)
            {
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _pipeClient.WaitForDrain();
                _task.WhereTo = "COMMAND";
                messageToAdd = new MessageModel
                {
                    Message = "BEGIN INTERROGATION",
                    Category = Category.Command,
                    TimeStamp = DateTime.Now
                };
                _task.Messages.Add(messageToAdd);
                _pipeClient = new PipeClient();
                _pipeClient.CreatePipe();
                _pipeClient.WriteString(_task.ToString());
                _pipeClient.WaitForDrain();
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Thread.Sleep(50);
                _task.WhereTo = "COMMAND";
                messageToAdd = new MessageModel
                {
                    Message = "BEGIN INTERROGATION",
                    Category = Category.Command,
                    TimeStamp = DateTime.Now
                };
                _task.Messages.Add(messageToAdd);
                _pipeClient = new PipeClient();
                _pipeClient.CreatePipe();
                _pipeClient.WriteString(_task.ToString());
                _pipeClient.WaitForDrain();
            }
        }
    }
}