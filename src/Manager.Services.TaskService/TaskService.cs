using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Manager.LIBS.Model;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace Manager.Services.TaskService
{
    public class TaskService : ITaskService
    {
        /// <summary>
        ///     String for the name of the Service
        /// </summary>
        private const string SERVICE_NAME = "Task Management Service";

        /// <summary>
        ///     String for the memory spot for the start
        /// </summary>
        private const string SERVICE_MEMORY = "TaskManagementServiceStarted";

        /// <summary>
        ///     String for the Pipe Name of the service
        /// </summary>
        private const string PIPE_NAME = "TASK";

        /// <summary>
        ///     ILoggerService used for the logger
        /// </summary>
        private readonly ILoggerService _loggerService;

        /// <summary>
        ///     IMemoryCache used to set and get from memory
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     PipeServer used to communicate with the Core
        /// </summary>
        private PipeServer _pipeServer;

        /// <summary>
        ///     Sorted Dictionary of the tasks in Priority
        /// </summary>
        private SortedDictionary<decimal, TaskModel> _priorities;

        /// <summary>
        ///     TaskService(...): Constructor for Task Service. Sets the Memory
        ///     Cache and Logger to Interactive Console
        /// </summary>
        /// <param name="memoryCache">
        ///     The Memory Cache to set
        /// </param>
        /// <param name="loggerService">
        ///     Requires a <see cref="ILoggerService" />
        /// </param>
        public TaskService(IMemoryCache memoryCache,
            ILoggerService loggerService)
        {
            _loggerService = loggerService;
            _memoryCache = memoryCache;
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Starts Tasks Processing in this Task Service. Calls
        ///     functions ProcessTaskPriority(), ErrorProcessTasks(), and
        ///     SendTasks() to aid in task processing
        /// </summary>
        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
            else
            {
                _memoryCache.Set("taskQueue", new TaskQueue());

                _loggerService.LogMessage(SERVICE_NAME, "Started", LogLevel.Info);

                // Task Engine still running?
                //while ((bool)_memoryCache.Get("taskServiceStarted"))
                while (true)
                    try
                    {
                        // Process Task priority based on most important priority
                        ProcessTaskPriority();
                        // Check error flag in each Task
                        ProcessTasks();
                        // Send Task(s) to their Where-To
                        SendTasks();

                        //Thread.Sleep(1000);
                    }

                    catch (TaskServiceException tse)
                    {
                        _memoryCache.Set(SERVICE_MEMORY, false);
                        _loggerService.LogMessage(SERVICE_NAME, tse.Message, LogLevel.Error);

                        throw new TaskServiceException(tse.Message);
                    }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Stops Task Processing in this Task Service
        /// </summary>
        public void Stop()
        {
            _memoryCache.Set(SERVICE_MEMORY, false);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Restarts Task Processing in this Task Service
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
        ///     Processes Tasks based on highest priority.
        ///     If two or more tasks share the same priority, a new priority will
        ///     be assigned to each at a decimal (i.e. 1, 1.1, 1.2, ..., etc.)
        /// </summary>
        private void ProcessTaskPriority()
        {
            // New Sorted Dictionary for priorities (auto-sorting)
            _priorities = new SortedDictionary<decimal, TaskModel>();

            // Obtain the in memory Task Queue
            var tempQueue = (TaskQueue) _memoryCache.Get("taskQueue");

            // Are there Task(s) to process?
            //if (tempQueue.Queue.Count <= 0) return;
            // Priority Tracking (i.e. Has this specific priority been
            // assigned? Priorities must be unique)
            var priorityTracking = new SortedDictionary<decimal,
                List<decimal>>();

            // Loop through all Task(s) in in memory Task Queue
            foreach (var task in tempQueue.Queue)
            {
                // This is used to ignore Unknown tasks
                if (task.WhereFrom == "Unknown" ||
                    task.WhereTo == "Unknown") continue;
                try
                {
                    _priorities.Add(task.Priority, task);
                }

                catch (ArgumentException ae)
                {
                    // Two or more Tasks have the same priority
                    // (hence the ArugmentException from
                    // the Sorted Dictionary). Add
                    // the second and more Tasks with + .10 priority
                    if (priorityTracking.TryGetValue(task.Priority,
                        out var tempPriority))
                    {
                        _priorities.Add(tempPriority.Max() +
                                        Convert.ToDecimal(0.10), task);
                    }

                    // Fatal error. Pass Exception up the stack
                    else
                    {
                        _memoryCache.Set(SERVICE_MEMORY, false);
                        _loggerService.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);
                        throw new TaskServiceException(ae.Message);
                    }
                }

                try
                {
                    priorityTracking.Add(task.Priority,
                        new List<decimal> {task.Priority});
                }

                catch (ArgumentException ae)
                {
                    // Two or more Tasks have the same priority (hence the
                    // ArugmentException from the Sorted Dictionary). Add
                    // the second and more Tasks with + .10 priority
                    if (priorityTracking.TryGetValue(task.Priority,
                        out var tempTrack))
                    {
                        priorityTracking.Remove(task.Priority);

                        tempTrack.Add(tempTrack.Max() +
                                      Convert.ToDecimal(0.10));

                        priorityTracking.Add(task.Priority, tempTrack);
                    }

                    // Fatal error. Pass Exception up the stack
                    else
                    {
                        _memoryCache.Set(SERVICE_MEMORY, false);
                        _loggerService.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);

                        throw new TaskServiceException(ae.Message);
                    }
                }
            }
        }

        /// <summary>
        ///     SendTasks(): Sends Task(s) to their Where-Tos
        /// </summary>
        public void SendTasks()
        {
            var tempQueue = (TaskQueue) _memoryCache.Get("taskQueue");
            var tempBag = new TaskQueue();

            try
            {
                // Loop through all Priorities in Sorted Dictionary
                foreach (var task in _priorities.ToList())
                    // Created new Pipe Server to send Task to Where-To
                    using (_pipeServer = new PipeServer(task.Value.WhereTo))
                    {
                        try
                        {
                            _pipeServer.CreatePipe();
                            _loggerService.LogMessage(SERVICE_NAME,
                                task.Value.Name + " moving to " + task.Value.WhereTo, LogLevel.Info);
                        }
                        catch (ArgumentNullException ane)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);

                            throw new TaskServiceException(ane.Message);
                        }

                        catch (ArgumentOutOfRangeException aoore)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, aoore.Message, LogLevel.Error);

                            throw new TaskServiceException(aoore.Message);
                        }

                        catch (ArgumentException ae)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);

                            throw new TaskServiceException(ae.Message);
                        }

                        catch (PlatformNotSupportedException pnse)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, pnse.Message, LogLevel.Error);

                            throw new TaskServiceException(pnse.Message);
                        }

                        catch (NotSupportedException nse)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);

                            throw new TaskServiceException(nse.Message);
                        }

                        catch (IOException ioe)
                        {
                            _memoryCache.Set("taskServiceStarted", false);
                            _loggerService.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);

                            throw new TaskServiceException(ioe.Message);
                        }

                        try
                        {
                            // Send Task to Where-To as a request
                            _pipeServer.WriteString(task.Value.ToString());
                        }

                        catch (NotSupportedException nse)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);

                            throw new TaskServiceException(nse.Message);
                        }

                        catch (ObjectDisposedException ode)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ode.Message, LogLevel.Error);

                            throw new TaskServiceException(ode.Message);
                        }

                        catch (ArgumentNullException ane)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);

                            throw new TaskServiceException(ane.Message);
                        }

                        catch (ArgumentOutOfRangeException aoofre)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, aoofre.Message, LogLevel.Error);

                            throw new TaskServiceException(aoofre.Message);
                        }

                        catch (IOException ioe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);

                            throw new TaskServiceException(ioe.Message);
                        }

                        catch (EncoderFallbackException efe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, efe.Message, LogLevel.Error);

                            throw new TaskServiceException(efe.Message);
                        }

                        catch (ArgumentException ae)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ae.Message, LogLevel.Error);

                            throw new TaskServiceException(ae.Message);
                        }

                        catch (OverflowException oe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, oe.Message, LogLevel.Error);

                            throw new TaskServiceException(oe.Message);
                        }

                        try
                        {
                            // Wait for Pipe to Drain
                            _pipeServer.WaitForDrain();

                            // Disconnects the pipeserver
                            _pipeServer.Disconnect();
                        }

                        catch (ObjectDisposedException ode)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ode.Message, LogLevel.Error);

                            throw new TaskServiceException(ode.Message);
                        }

                        catch (NotSupportedException nse)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, nse.Message, LogLevel.Error);

                            throw new TaskServiceException(nse.Message);
                        }

                        catch (IOException ioe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);

                            throw new TaskServiceException(ioe.Message);
                        }

                        catch (InvalidOperationException ioe)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ioe.Message, LogLevel.Error);

                            throw new TaskServiceException(ioe.Message);
                        }

                        // Close Pipe that send this Task to Where-To
                        _pipeServer.ClosePipe();

                        try
                        {
                            // Remove Task from Priorities
                            _priorities.Remove(task.Key);
                        }
                        catch (ArgumentNullException ane)
                        {
                            _memoryCache.Set(SERVICE_MEMORY, false);
                            _loggerService.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);

                            throw new TaskServiceException(ane.Message);
                        }

                        // Since Concurrent Bag does not have a remove function,
                        // Shadow copy all Task(s) that were NOT sent and copy
                        // that Concurrent Bag back into memory
                        foreach (var temp in tempQueue.Queue)
                            if (temp.Name != task.Value.Name)
                                tempBag.Queue.Add(temp);

                        tempQueue = tempBag;
                        tempBag = new TaskQueue();
                    }
            }

            catch (ArgumentNullException ane)
            {
                _memoryCache.Set(SERVICE_MEMORY, false);
                _loggerService.LogMessage(SERVICE_NAME, ane.Message, LogLevel.Error);

                throw new TaskServiceException(ane.Message);
            }

            _memoryCache.Set("taskQueue", tempQueue);
        }

        /// <summary>
        ///     Process errors witin currently received
        ///     Task(s). If an error is present, change the Where-To
        /// </summary>
        private void ProcessTasks()
        {
            // Loop through all Task(s) in Priorites Sorted Dictionary
            foreach (var task in _priorities.ToList())
            {
                // Check if Value is null
                if (task.Value.Messages == null) continue;
                // Loop through all Message(s) within Task(s)
                foreach (var message in task.Value.Messages)
                    // If Category is error, switch whereto to move
                    // the task to the error processor.

                    if (message.Category == Category.Error)
                    {
                        task.Value.WhereTo = "ERROR";
                        break;
                    }
                    else
                    {
                        switch (message.Category)
                        {
                            case Category.Start:
                                task.Value.WhereTo = "INTERROGATOR";
                                break;
                            case Category.Command:
                                task.Value.WhereTo = "COMMAND";
                                break;
                            case Category.Regex:
                                task.Value.WhereTo = "REGEX";
                                break;
                            case Category.FinalResult:
                                task.Value.WhereTo = "FINAL_RESULT";
                                break;
                        }
                    }
            }
        }
    }
}