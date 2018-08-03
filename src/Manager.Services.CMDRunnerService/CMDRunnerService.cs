using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace Manager.Services.CMDRunnerService
{
    public class CmdRunnerService : ICmdRunnerService
    {
        /// <summary>
        ///     String of the Serivce Name
        /// </summary>
        private const string SERVICE_NAME = "Command Runner Service";

        /// <summary>
        ///     String of the Service Memory spot if service is started
        /// </summary>
        private const string SERVICE_MEMORY = "CommandRunnerServiceStarted";

        /// <summary>
        ///     String for the Pipe Name
        /// </summary>
        private const string PIPE_NAME = "COMMAND";

        /// <summary>
        ///     String for the key for the dictionary being used to run command
        /// </summary>
        private const string CMD_TO_RUN = "commandToRun";

        /// <summary>
        ///     String for the key for the direction being used for the args of the command
        /// </summary>
        private const string CMD_ARGS = "args";

        /// <summary>
        ///     ILoggerService to be used for logging
        /// </summary>
        private readonly ILoggerService _loggerService;

        /// <summary>
        ///     IMemoryCache to be used for checking if service is started or disabled
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     Used as the message to add to a task of the results
        /// </summary>
        private MessageModel _messageToAdd;

        /// <summary>
        ///     Used as the pipe client for getting communication
        /// </summary>
        private PipeClient _pipeClient;

        /// <summary>
        ///     Used as the pipe server for getting information back to the core
        /// </summary>
        private PipeServer _pipeServer;

        /// <summary>
        ///     The results of running the command
        /// </summary>
        private string _result;

        /// <summary>
        ///     The task that is being processed
        /// </summary>
        private TaskModel _taskToProcess;

        /// <summary>
        ///     Constructor for the command runner service
        /// </summary>
        /// <param name="loggerService">
        ///     Requires an ILoggerService for logging
        /// </param>
        /// <param name="memoryCache">
        ///     Requires an IMemoryCache for communicating with memory
        /// </param>
        public CmdRunnerService(ILoggerService loggerService,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _loggerService = loggerService;
            _pipeClient = new PipeClient(PIPE_NAME);
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        public string Cmd { get; set; }
        public string Args { get; set; }

        /// <summary>
        ///     Used as the start of the service
        /// </summary>
        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
                _loggerService.LogMessage(SERVICE_NAME, "Started", LogLevel.Info);
            }

            while ((bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _loggerService.LogMessage(SERVICE_NAME, "Started", LogLevel.Info);

                _pipeClient.CreatePipe();


                var incomingTask = _pipeClient.ReadString();
                _loggerService.LogMessage(SERVICE_NAME, incomingTask, LogLevel.Info);
                _taskToProcess = new TaskModel(incomingTask);
                ProcessTask();
                _pipeClient.WaitForDrain();
                _pipeClient = new PipeClient(PIPE_NAME);
                _pipeServer = new PipeServer();

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        ///     Used for restarting the service
        /// </summary>
        public void Restart()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
                _loggerService.LogMessage(SERVICE_NAME, "Started", LogLevel.Info);
            }
            else
            {
                Stop();
                Start();
            }
        }

        /// <summary>
        ///     Used for stopping the service
        /// </summary>
        public void Stop()
        {
            _memoryCache.Set(SERVICE_MEMORY, false);
        }

        /// <summary>
        ///     This processes the task that was send to the pipe
        /// </summary>
        private void ProcessTask()
        {
            var cmdsToRun = _taskToProcess.Messages.Where(t => t.Category == Category.Command);
            var stressMessage = _taskToProcess.Messages.Where(s => s.Category == Category.Stress);
            var stressSystem = stressMessage.Any(stressMsg => stressMsg.Message == "ALL");
            foreach (var command in cmdsToRun)
                if (command.Message == "BEGIN INTERROGATION")
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        var interrogatorCmd = "wscript.exe " + Path.GetTempPath() + Path.DirectorySeparatorChar +
                                              "Manager" +
                                              Path.DirectorySeparatorChar + "driveinfo.vbs";
                        RunCommand(ProcessCmd(interrogatorCmd));


                        if (stressSystem)
                        {
                            var stressCmd = "wscript.exe " + Path.GetTempPath() + Path.DirectorySeparatorChar +
                                            "Manager" +
                                            Path.DirectorySeparatorChar + "ReadFio.vbs";

                            RunCommand(ProcessCmd(stressCmd));
                        }

                        break;
                    }
                    else
                    {
                        var interrogatorCmd = "python " + Path.GetTempPath() + Path.DirectorySeparatorChar + "Manager" +
                                              Path.DirectorySeparatorChar + "find_block_devices.py";
                        RunCommand(ProcessCmd(interrogatorCmd));
                    }
                }
                else
                {
                    var endCmd = ProcessCmd(command.Message);
                    RunCommand(endCmd);
                    _messageToAdd = new MessageModel
                    {
                        Category = Category.TestResult,
                        Message = _result,
                        TimeStamp = DateTime.Now
                    };
                    _taskToProcess.Messages.Add(_messageToAdd);
                    var startMessage = _taskToProcess.Messages.Single(r => r.Category == Category.Start);
                    break;
                }
        }

        /// <summary>
        ///     Processes the string text so that the command and args are split
        /// </summary>
        /// <param name="text">Requires a string of the text</param>
        /// <returns>Returns a Dictionary <string, string> </returns>
        private Dictionary<string, string> ProcessCmd(string text)
        {
            var cmd = new Dictionary<string, string>();
            //Process the Text
            var cmdWithArgs = text.Split(' ');
            cmd.Add(CMD_TO_RUN, cmdWithArgs[0]);
            Args = "";
            // Process the Args
            for (var i = 1; i < cmdWithArgs.Length; i++) Args += ' ' + cmdWithArgs[i];

            if (cmd.ContainsKey(CMD_ARGS))
                cmd[CMD_ARGS] = Args;
            else
                cmd.Add(CMD_ARGS, Args);

            return cmd;
        }

        /// <summary>
        ///     Runs a command based on a Dictionary with string, string
        /// </summary>
        /// <param name="endCmd">Requires a dictionary of string, string</param>
        private void RunCommand(IReadOnlyDictionary<string, string> endCmd)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = endCmd[CMD_TO_RUN],
                        Arguments = endCmd[CMD_ARGS],
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = false
                    }
                };
                var thread = new Thread(() =>
                {
                    proc.Start();
                    _result = "";
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        _result += line;
                    }

                    _messageToAdd = new MessageModel
                    {
                        Category = Category.TestResult,
                        Message = _result,
                        TimeStamp = DateTime.Now
                    };
                    _taskToProcess.Messages.Add(_messageToAdd);
                    //_loggerService.LogMessage(SERVICE_NAME, "Return Task: " + _taskToProcess, LogLevel.Info);
                    if (!string.IsNullOrEmpty(_messageToAdd.Message)) SendResultToCore();
                    //_loggerService.LogMessage(SERVICE_NAME, "Result from Command: " + _result, LogLevel.Info);
                });
                thread.Start();
            }
            else
            {
                var cmd = endCmd[CMD_TO_RUN] + " " + endCmd[CMD_ARGS];
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{cmd}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                var thread = new Thread(() =>
                {
                    proc.Start();
                    _result = "";
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        _result += line;
                    }

                    _messageToAdd = new MessageModel
                    {
                        Category = Category.TestResult,
                        Message = _result,
                        TimeStamp = DateTime.Now
                    };
                    _taskToProcess.Messages.Add(_messageToAdd);

                    SendResultToCore();
                });
                thread.Start();
            }
        }

        /// <summary>
        ///     Used to send a task back to the core.
        /// </summary>
        private void SendResultToCore()
        {
            _loggerService.LogMessage(SERVICE_NAME, _taskToProcess.ToString(), LogLevel.Info);

            _pipeServer = new PipeServer();
            _pipeServer.CreatePipe();
            _pipeServer.WriteString(_taskToProcess.ToString());
            _pipeServer.WaitForDrain();
        }
    }
}