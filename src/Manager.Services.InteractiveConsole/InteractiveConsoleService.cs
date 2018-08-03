using System;
using System.Collections.Generic;
using System.Threading;
using Manager.Services.InteractiveConsoleService.DrawScreen;
using Manager.Services.LoggerService;
using Manager.Services.PipeService;
using Manager.Services.TaskService;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using NLog.Targets;

namespace Manager.Services.InteractiveConsoleService
{
    public class InteractiveConsoleService : IInteractiveConsoleService
    {
        // Core starting in progress message
        private const string StartReceipt = "Starting Core...";

        // Core stopping in progress message
        private const string StopReceipt = "Stopping Core...";

        // Core restarting in progress message
        private const string RestartReceipt = "Restarting Core...";

        // Core exiting in progress message
        private const string ExitingReceipt = "Exiting Core...";

        // Core started message
        private const string StartConfirm = "Started Core Successfully.";

        // Core stopped message
        private const string StopConfirm = "Stopped Core Successfully.";

        // Core restarted message
        private const string RestartConfirm = "Restarted Core Successfully.";

        // Core failed to start message
        private const string StartFail = "Failed to Start Core.";

        // Core failed to stop message
        private const string StopFail = "Failed to Stop Core.";

        // Core failed to restart message
        private const string RestartFail = "Failed to Restart Core.";

        // Core failed to exit message
        private const string ExitFail = "Failed to Exit Core.";

        // Core already started message
        private const string StartAlready = "Core already Started.";

        // Core already stopped message
        private const string StopAlready = "Core already Stopped.";

        // Help Menu message
        private const string HelpMenu = "Core CLI Usage:\r\n" +
                                        "start      - Starts the Core immediately\r\n" +
                                        "stop       - Stops the Core immediately\r\n" +
                                        "restart    - Restarts the Core after all current Task processing\r\n" +
                                        "clear      - Clears the interactive console (bottom window)\r\n" +
                                        "clear logs - Clears the interactive console logs (top window)\r\n" +
                                        "exit       - Exits the Core after all current Task processing\r\n" +
                                        "help       - Prints this Core Help Menu\r\n";

        // CLI Screen name in memory
        private const string CliScreen = "cliScreen";

        // Log Screen name in memory
        private const string LogScreen = "logScreen";

        private const string SERVICE_NAME = "Interactive Console Service";

        private const string SERVICE_MEMORY = "InteractiveConsoleServiceStarted";

        private const string PIPE_NAME = "INTERACTIVECONSOLE";

        // Draw Screen Service dependency injection
        private readonly IDrawScreen _drawScreen;

        private readonly ILoggerService _loggerService;

        // Interactive Console Logger Service dependency injection
        //private readonly ILogToInteractiveConsole _logToInteractiveConsole;

        // Memory Cache Service dependency injection
        private readonly IMemoryCache _memoryCache;

        // Pipe Service dependency injection
        private readonly IPipeService _pipeService;

        // Memory Target for log in memory
        private readonly MemoryTarget _target;

        // Pipe Service dependency injection
        private readonly ITaskService _taskService;

        /// <summary>
        ///     Constructor for Interactive Console.
        ///     Starts the Memory Cache, Draw Screen, Pipe Server, Task Engine,
        ///     and Log to Interactive Console
        /// </summary>
        /// <param name="memoryCache">The Memory Cache Service to set</param>
        /// <param name="drawScreen">The Draw Screen Service to set</param>
        /// <param name="pipeServer">The Pipe Server Service to set</param>
        /// <param name="taskEngine">The Task Engine Service to set</param>
        /// <param name="loggerService">
        ///     The Interactive Log Service
        ///     to set
        /// </param>
        /// <param name="logToInteractiveConsole"></param>
        public InteractiveConsoleService(IMemoryCache memoryCache,
            IDrawScreen drawScreen, IPipeService pipeServer,
            ITaskService taskEngine,
            ILoggerService loggerService)
        {
            _memoryCache = memoryCache;
            _drawScreen = drawScreen;
            _pipeService = pipeServer;
            _taskService = taskEngine;
            _loggerService = loggerService;
            _target = LogManager.Configuration.FindTargetByName<MemoryTarget>("logMemory");
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Starts the Interactive CLI by setting a Core Started?
        ///     boolean flag, CLI sub-window history list, and Log sub-window
        ///     history list. Starts Console Writer as well
        /// </summary>
        public void Start()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(CliScreen, new List<string>());
                _memoryCache.Set(LogScreen, new List<string>());

                ConsoleWriter();
            }
            else
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
        }

        /// <summary>
        ///     Stops the Interactive Console Service
        /// </summary>
        public void Stop()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY)) _memoryCache.Set(SERVICE_MEMORY, false);
        }

        /// <summary>
        ///     Restarts the interactive console
        /// </summary>
        public void Restart()
        {
            if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                Stop();
                Start();
            }
            else
            {
                Start();
            }
        }

        /// <summary>
        ///     Draws the entire Console screen window over and
        ///     over to produce the user perception of using an interactive CLI
        /// </summary>
        private void ConsoleWriter()
        {
            // Retrieve what the current Console Window Height is to produce
            // the CLI and Log screen heights for sub-windows
            _drawScreen.ScreenHeights = (Console.WindowHeight - 2) / 2;

            _drawScreen.ScreenWidths = Console.WindowWidth;

            // Draw the entire Console screens right away so end user sees
            // something
            _drawScreen.DrawWholeScreenNow();

            // Loop forever until Core is exited out (either manually or
            // through CLI command)
            while (true)
            {
                // Read in User Input as CLI command
                var userInput = Console.ReadLine();

                // Retrieve CLI screen "list" memory of commands
                var tempCliArea = (List<string>) _memoryCache.Get(CliScreen);

                // Add the User's Input into "list" memory of commands to draw
                AddLineToBuffer(tempCliArea, ">" + userInput);

                // Determine user's command input
                switch (userInput)
                {
                    // Clear the Interactive CLI sub-window
                    case "clear":

                        _memoryCache.Set(CliScreen, new List<string>());
                        break;

                    // Clear the Log sub-window
                    case "clear logs":

                        ClearLogs();
                        break;

                    // Exits the Core in its entirety
                    case "exit":

                        ExitCore();
                        break;

                    // Prints Help menu to Interactive CLI for user
                    case "help":

                        Help();
                        break;

                    // Restarts the Core
                    case "restart":

                        RestartCore();
                        break;

                    // Starts the Core
                    case "start":

                        StartCore();
                        break;

                    // Stops the Core
                    case "stop":

                        StopCore();
                        break;

                    // Default prints Help menu to Interactive CLI for user
                    default:

                        Help();
                        break;
                }

                // Throttle Screen writing to limit user perception of
                // "blinking"
                Thread.Sleep(50);
                //_loggerService.LogMessage("Interactive Console Service: Started", LogLevel.Debug);

                // Draw the screen
                _drawScreen.DrawCliScreenNow();
            }
        }

        /// <summary>
        ///     Starts the Core by starting the Pipe Service and Task
        ///     Service
        /// </summary>
        private void StartCore()
        {
            // Is Core not already started?
            if ((bool) _memoryCache.Get(SERVICE_MEMORY) == false)
            {
                // Print appropriate message
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    StartReceipt);

                _loggerService.LogMessage(SERVICE_NAME,
                    "Starting Pipe Server Core Service...",
                    LogLevel.Debug
                );

                var startPipe = new int();

                // Spin up new Thread for Pipe Server Service Start
                new Thread(() =>
                {
                    try
                    {
                        _pipeService.Start();
                    }

                    catch (PipeServiceException pse)
                    {
                        // Print appropriate message
                        AddLineToBuffer((List<string>)
                            _memoryCache.Get(CliScreen), StartFail);

                        _loggerService.LogMessage(SERVICE_NAME,
                            pse.Message,
                            LogLevel.Error
                        );

                        startPipe = -1;

                        return;
                    }

                    startPipe = 0;
                }).Start();

                if (startPipe < 0) return;

                _loggerService.LogMessage(SERVICE_NAME,
                    "Starting Task Core Service...", LogLevel.Debug);

                var startTask = new int();

                // Spin up new Thread for Task Service Start
                new Thread(() =>
                {
                    try
                    {
                        _taskService.Start();
                    }

                    catch (TaskServiceException tse)
                    {
                        // Print appropriate message
                        AddLineToBuffer((List<string>)
                            _memoryCache.Get(CliScreen), StartFail);

                        _loggerService.LogMessage(SERVICE_NAME,
                            tse.Message,
                            LogLevel.Error
                        );

                        startTask = -1;

                        return;
                    }

                    startTask = 0;
                }).Start();

                if (startTask < 0) return;

                // Set Core Started boolean
                _memoryCache.Set(SERVICE_MEMORY, true);

                // Print appropriate message
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    StartConfirm);
            }

            // Core already started, print appropriate message
            else
            {
                // Print appropriate message
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    StartAlready);
            }
        }

        /// <summary>
        ///     Stops the Core by stopping the Pipe Service and Task Service
        /// </summary>
        private void StopCore()
        {
            // Print appropriate message
            AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                StopReceipt);

            // Is the Core not started?
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                // Print appropriate message
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    StopAlready);

                _loggerService.LogMessage(SERVICE_NAME, StopAlready, LogLevel.Debug);

                return;
            }


            _loggerService.LogMessage(SERVICE_NAME,
                "Stopping Pipe Server Core Service...", LogLevel.Debug);

            // Stop Pipe Server Service
            try
            {
                _pipeService.Stop();
            }

            catch (PipeServiceException pse)
            {
                // Print appropriate message
                AddLineToBuffer((List<string>)
                    _memoryCache.Get(CliScreen), StopConfirm);

                _loggerService.LogMessage(SERVICE_NAME,
                    pse.Message,
                    LogLevel.Error
                );

                _memoryCache.Set(SERVICE_MEMORY, false);

                return;
            }

            _loggerService.LogMessage(SERVICE_NAME,
                "Stopping Task Core Service...", LogLevel.Debug);

            // Stop Task Service
            try
            {
                _taskService.Stop();
            }

            catch (TaskServiceException tse)
            {
                // Print appropriate message
                AddLineToBuffer((List<string>)
                    _memoryCache.Get(CliScreen), StopConfirm);

                _loggerService.LogMessage(SERVICE_NAME,
                    tse.Message,
                    LogLevel.Error
                );

                _memoryCache.Set(SERVICE_MEMORY, false);

                return;
            }

            _memoryCache.Set(SERVICE_MEMORY, false);

            // Print appropriate message
            AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                StopConfirm);
        }


        /// <summary>
        ///     Restarts the Core by stopping then starting the
        ///     Pipe Service and Task Service
        /// </summary>
        private void RestartCore()
        {
            // Is the Core not started?
            if ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                // Print appropriate message
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    RestartReceipt);

                _loggerService.LogMessage(SERVICE_NAME,
                    "Stopping Pipe Server Core Service...",
                    LogLevel.Debug
                );

                // Stop Pipe Server Service
                try
                {
                    _pipeService.Stop();
                }

                catch (PipeServiceException pse)
                {
                    // Print appropriate message
                    AddLineToBuffer((List<string>)
                        _memoryCache.Get(CliScreen), RestartFail);

                    _loggerService.LogMessage(SERVICE_NAME,
                        pse.Message,
                        LogLevel.Error
                    );

                    return;
                }

                _loggerService.LogMessage(SERVICE_NAME,
                    "Stopping Task Core Service...", LogLevel.Debug);

                // Stop Task Service
                try
                {
                    _taskService.Stop();
                }

                catch (TaskServiceException tse)
                {
                    // Print appropriate message
                    AddLineToBuffer((List<string>)
                        _memoryCache.Get(CliScreen), RestartConfirm);

                    _loggerService.LogMessage(SERVICE_NAME,
                        tse.Message,
                        LogLevel.Error
                    );

                    return;
                }
            }

            // Set Core Started boolean
            _memoryCache.Set(SERVICE_MEMORY, false);

            _loggerService.LogMessage(SERVICE_NAME,
                "Starting Pipe Server Core Service...", LogLevel.Debug);

            var startPipe = new int();

            // Spin up new Thread for Pipe Server Service Start
            new Thread(() =>
            {
                try
                {
                    _pipeService.Start();
                }

                catch (PipeServiceException pse)
                {
                    // Print appropriate message
                    AddLineToBuffer((List<string>)
                        _memoryCache.Get(CliScreen), RestartConfirm);

                    _loggerService.LogMessage(SERVICE_NAME,
                        pse.Message,
                        LogLevel.Error
                    );

                    startPipe = -1;

                    return;
                }

                startPipe = 0;
            }).Start();

            if (startPipe < 0) return;

            _loggerService.LogMessage(SERVICE_NAME,
                "Starting Task Core Service...", LogLevel.Debug);

            var startTask = new int();

            // Spin up new Thread for Task Service Start
            new Thread(() =>
            {
                try
                {
                    _taskService.Start();
                }

                catch (TaskServiceException tse)
                {
                    // Print appropriate message
                    AddLineToBuffer((List<string>)
                        _memoryCache.Get(CliScreen), RestartFail);

                    _loggerService.LogMessage(SERVICE_NAME,
                        tse.Message,
                        LogLevel.Error
                    );

                    startTask = -1;

                    return;
                }

                startTask = 0;
            }).Start();

            if (startTask < 0) return;

            AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                RestartConfirm);

            // Set Core Started boolean
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     ExitCore(): Exits the Core altogether stopping the Pipe Service
        ///     and Task Service then Exits the currently executing process
        /// </summary>
        private void ExitCore()
        {
            // Print appropriate message
            AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                ExitingReceipt);

            _loggerService.LogMessage(SERVICE_NAME,
                "Stopping Pipe Server Core Service...", LogLevel.Debug);

            // Stop Pipe Server Service
            try
            {
                _pipeService.Stop();
            }

            catch (PipeServiceException pse)
            {
                // Print appropriate message
                AddLineToBuffer((List<string>)
                    _memoryCache.Get(CliScreen), StopFail);

                _loggerService.LogMessage(SERVICE_NAME,
                    pse.Message,
                    LogLevel.Error
                );

                return;
            }

            _loggerService.LogMessage(SERVICE_NAME,
                "Stopping Task Core Service...", LogLevel.Debug);

            // Stop Task Service
            try
            {
                _taskService.Stop();
            }

            catch (TaskServiceException tse)
            {
                // Print appropriate message
                AddLineToBuffer((List<string>)
                    _memoryCache.Get(CliScreen), StopFail);

                _loggerService.LogMessage(SERVICE_NAME,
                    tse.Message,
                    LogLevel.Error
                );

                return;
            }

            // Draw Screen for User to see appropriate message
            _drawScreen.DrawCliScreenNow();

            Thread.Sleep(1000);

            // Exit currently executing process
            Environment.Exit(0);
        }

        /// <summary>
        ///     Prints the help message to Interactive CLI for end user
        /// </summary>
        private void Help()
        {
            // Split help message on carriage return and newline as CLI
            // doesn't accept them
            var delimeter = new[] {"\r\n"};

            // Help message lines
            var result = HelpMenu.Split(delimeter,
                StringSplitOptions.RemoveEmptyEntries);

            // Print each help message line
            foreach (var item in result)
                AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen),
                    item);

            AddLineToBuffer((List<string>) _memoryCache.Get(CliScreen), "\r\n");
        }

        /// <summary>
        ///     Adds a line to a specific screen list to
        ///     print on Console sub-window
        /// </summary>
        /// <param name="screenBuffer">The Screen list to print to</param>
        /// <param name="line">The string text to print</param>
        public void AddLineToBuffer(List<string> screenBuffer, string line)
        {
            // Always insert at beginning of Screen buffer list
            screenBuffer.Insert(0, line);

            // Remove last element in Screen buffer list
            if (screenBuffer.Count == _drawScreen.ScreenHeights) screenBuffer.RemoveAt(_drawScreen.ScreenHeights - 1);
        }

        /// <summary>
        ///     Clears the logs from the memory target.
        /// </summary>
        private void ClearLogs()
        {
            _target.Logs.Clear();

            _drawScreen.DrawLogScreenNow();
        }
    }
}