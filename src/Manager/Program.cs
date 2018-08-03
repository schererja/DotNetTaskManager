using System;
using System.IO;
using Manager.Application;
using Manager.Services.CMDRunnerService;
using Manager.Services.ErrorProcessingService;
using Manager.Services.InteractiveConsoleService;
using Manager.Services.InteractiveConsoleService.DrawScreen;
using Manager.Services.InteractiveConsoleService.Logger;
using Manager.Services.InterrogatorService;
using Manager.Services.LoggerService;
using Manager.Services.PipeService;
using Manager.Services.TaskService;
using Manager.Services.TCPService;
using Manager.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Manager
{
    /// <summary>
    ///     Main startup program
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     Used as the log config file name
        /// </summary>
        private const string LoggerConfigFile = "nlog.config";

        /// <summary>
        ///     Used as the application config file name
        /// </summary>
        private const string AppConfigFile = "app-settings.json";

        /// <summary>
        ///     Main function, needed to begin application
        /// </summary>
        /// <param name="args">Takes an array of strings</param>
        private static void Main(string[] args)
        {
            // Args if exist
            uint serialNumber = 0;

            var shouldStart = false;
            var shouldStress = false;
            // Creates the Service Collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Creates the Service Provider
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>()
                .AddNLog(new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
            // Loads the internal configuration file
            LogManager.LoadConfiguration(LoggerConfigFile);

            // If args is above 0 grab out the serial number as the first args
            if (args.Length > 0)
            {
                var isInt = uint.TryParse(args[0], out serialNumber);
                if (args.Length > 1)
                {
                    if (args.Length > 2)
                    {
                        var isStress = string.IsNullOrEmpty(args[2]);
                        if (!isStress && args[2] == "STRESS") shouldStress = true;
                    }

                    var isStart = string.IsNullOrEmpty(args[1]);
                    if (!isStart && args[1] == "START") shouldStart = true;
                }

                if (!isInt)
                    Usage();
                else
                    serviceProvider.GetService<App>().Run(serialNumber, shouldStart, shouldStress);
            }
            else
            {
                // Used to start the application with a serial number of 0 if
                // no serial number was provided
                serviceProvider.GetService<App>().Run(serialNumber, false, false);
            }
        }

        /// <summary>
        ///     Used to setup the configuration of the services that are being used
        /// </summary>
        /// <param name="serviceCollection">Requires an IServiceCollection</param>
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddDebug());

            serviceCollection.AddLogging();

            // Sets up configuration settings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppConfigFile, false)
                .Build();

            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration
                .GetSection("Configuration"));

            // Services getting added
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<IPipeService, PipeService>();
            serviceCollection.AddSingleton<ITaskService, TaskService>();
            serviceCollection.AddSingleton<IInteractiveConsoleService,
                InteractiveConsoleService>();
            serviceCollection.AddSingleton<IDrawScreen, DrawScreen>();
            serviceCollection.AddSingleton<ILogToInteractiveConsole,
                LogToInteractiveConsole>();
            serviceCollection.AddSingleton<ILoggerService, LoggerService>();
            serviceCollection.AddTransient<IErrorProcessingService,
                ErrorProcessingService>();
            serviceCollection.AddSingleton<ITcpService, TcpService>();
            serviceCollection.AddSingleton<ICmdRunnerService, CmdRunnerService>();
            serviceCollection.AddSingleton<IInterrogatorService, InterrogatorService>();


            // Add the application itself
            serviceCollection.AddTransient<App>();
        }

        /// <summary>
        ///     Used for giving a usage statement
        /// </summary>
        private static void Usage()
        {
            Console.Write("Please supply a serial number");
        }
    }
}