using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace Manager.Services.ParserService
{
    public class ParserService : IParserService
    {
        private const string SERVICE_NAME = "Parser Service";
        private const string SERVICE_MEMORY = "ParserServiceStarted";
        private const string PIPE_NAME = "PARSER";
        private readonly ILoggerService _loggerService;
        private readonly IMemoryCache _memoryCache;
        private readonly PipeClient _pipeClient;
        private TaskModel _taskToProcess;

        public ParserService(ILoggerService loggerService,
            IMemoryCache memoryCache)
        {
            _loggerService = loggerService;
            _memoryCache = memoryCache;
            _memoryCache.Set(SERVICE_MEMORY, true);
            _pipeClient = new PipeClient(PIPE_NAME);
        }

        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY)) _memoryCache.Set(SERVICE_MEMORY, true);
            BringUpPipe();
            ProcessTask();
        }

        public void Stop()
        {
            _memoryCache.Set(SERVICE_MEMORY, false);
        }

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

        private void BringUpPipe()
        {
            _pipeClient.CreatePipe();
            var incomingTask = _pipeClient.ReadString();
            _loggerService.LogMessage(SERVICE_NAME, incomingTask, LogLevel.Info);
            _taskToProcess = new TaskModel(incomingTask);
        }

        private void ProcessTask()
        {
        }
    }
}