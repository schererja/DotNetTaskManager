using System;
using System.Collections.Generic;
using System.Threading;
using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.CMDRunnerService;
using Manager.Services.ErrorProcessingService;
using Manager.Services.InteractiveConsoleService;
using Manager.Services.InterrogatorService;
using Manager.Services.LoggerService;
using Manager.Services.PipeService;
using Manager.Services.TaskService;
using Manager.Services.TCPService;
using Manager.Settings;
using Microsoft.Extensions.Options;

namespace Manager.Application
{
    /// <summary>
    ///     Main Application
    /// </summary>
    public class App
    {
        private readonly ICmdRunnerService _commandRunnerService;
        private readonly AppSettings _config;
        private readonly IErrorProcessingService _errorProcessingService;
        private readonly IInteractiveConsoleService _interactiveConsoleService;
        private readonly IInterrogatorService _interrogatorService;
        private readonly ILoggerService _loggerService;
        private readonly IPipeService _pipeService;
        private readonly ITaskService _taskService;
        private readonly ITcpService _tcpService;

        /// <summary>
        ///     Main Application that takes in all the services
        /// </summary>
        /// <param name="pipeService">Requires an <see cref="IPipeService" /></param>
        /// <param name="taskService">Requires an <see cref="ITaskService" /></param>
        /// <param name="interactiveConsoleService">Requires an <see cref="IInteractiveConsoleService" /></param>
        /// <param name="loggerService">Requires an <see cref="ILoggerService" /></param>
        /// <param name="errorProcessingService">Requires an <see cref="IErrorProcessingService" /></param>
        /// <param name="tcpService">Requires an <see cref="ITcpService" /></param>
        /// <param name="cmdRunnerService"></param>
        /// <param name="config">Requires an <see cref="IOptions{TOptions}" /></param>
        public App(IPipeService pipeService,
            ITaskService taskService,
            IInteractiveConsoleService interactiveConsoleService,
            ILoggerService loggerService,
            IErrorProcessingService errorProcessingService,
            ITcpService tcpService,
            ICmdRunnerService cmdRunnerService,
            IInterrogatorService interrogatorService,
            IOptions<AppSettings> config
        )
        {
            _interactiveConsoleService = interactiveConsoleService;
            _pipeService = pipeService;
            _taskService = taskService;
            _loggerService = loggerService;
            _errorProcessingService = errorProcessingService;
            _tcpService = tcpService;
            _commandRunnerService = cmdRunnerService;
            _interrogatorService = interrogatorService;
            _config = config.Value;
        }

        private uint SerialNumber { get; set; }

        /// <summary>
        ///     Starts up all the services that are enabled in the configuration
        /// </summary>
        /// <param name="serialNumber">Requires an <see cref="int" /></param>
        public void Run(uint serialNumber, bool shouldStart, bool shouldStress)
        {
            SerialNumber = serialNumber;
            foreach (var component in _config.Components)
                switch (component.Name)
                {
                    case "InteractiveConsoleService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _interactiveConsoleService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "PipeService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _pipeService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "TaskService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _taskService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "TCPService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _tcpService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "ErrorProcessingService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _errorProcessingService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "CMDRunnerService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _commandRunnerService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    case "InterrogatorService":
                        if (component.Properties.Enabled)
                            new Thread(() =>
                            {
                                _interrogatorService.Start();
                                //Thread.Sleep(1000);
                            }).Start();

                        break;
                    default:
                        break;
                }

            if (!shouldStart && !shouldStress) return;
            if (!shouldStart) return;
            CreateStartTask(shouldStress);
        }

        /// <summary>
        ///     Creates a starting task and if stress is passed it will added
        ///     in to the starting task as another message
        /// </summary>
        /// <param name="shouldStress">Requires a boolean for shouldStress</param>
        private void CreateStartTask(bool shouldStress)
        {
            var messages = new List<MessageModel>();
            var startMessage = new MessageModel
            {
                Category = Category.Start,
                Message = "Start Testing",
                TimeStamp = DateTime.Now
            };
            var startTask = new TaskModel
            {
                Guid = Guid.NewGuid(),
                Messages = messages,
                Name = "Start",
                Priority = 1,
                SerialNumber = SerialNumber,
                Timeout = 60,
                TimeStamp = DateTime.Now,
                WhereFrom = "CORE",
                WhereTo = "INTERROGATOR"
            };
            messages.Add(startMessage);
            if (shouldStress)
            {
                var stressMessage = new MessageModel
                {
                    Category = Category.Stress,
                    Message = "ALL",
                    TimeStamp = DateTime.Now
                };

                messages.Add(stressMessage);
            }

            var pipeServer = new PipeClient();
            pipeServer.CreatePipe();
            pipeServer.WriteString(startTask.ToString());
            pipeServer.WaitForDrain();
        }
    }
}