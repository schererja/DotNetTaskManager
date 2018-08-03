using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Manager.LIBS.Pipe;
using Manager.Services.LoggerService;
using Manager.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Manager.Services.TCPService

{
    public class TcpService : ITcpService
    {
        //private TaskModel _taskToProcess;

        private const string SERVICE_NAME = "TCP Service";
        private const string SERVICE_MEMORY = "TCPServiceStarted";

        private const string PIPE_NAME = "TCP";

        // THIS WILL BE USED FOR SECURE SSL Transfer
        //private static X509Certificate _serverCertificate = null;
        private readonly AppSettings _config;

        private readonly ILoggerService _loggerService;
        private readonly IMemoryCache _memoryCache;
        private PipeClient _pipeClient;
        private PipeServer _pipeServer;

        public TcpService(ILoggerService loggerService,
            IOptions<AppSettings> config,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _config = config.Value;
            _loggerService = loggerService;
            _pipeClient = new PipeClient();
            _pipeServer = new PipeServer(PIPE_NAME);
            _memoryCache.Set(SERVICE_MEMORY, true);
        }

        /// <summary>
        ///     Used to start the Service
        /// </summary>
        public void Start()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
                _memoryCache.Set(SERVICE_MEMORY, true);
            else
                while ((bool) _memoryCache.Get(SERVICE_MEMORY))
                    RunServer();
        }

        /// <summary>
        ///     Used to stop the service
        /// </summary>
        public void Stop()
        {
            _memoryCache.Set(SERVICE_MEMORY, false);
        }

        /// <summary>
        ///     Used to restart the service
        /// </summary>
        public void Restart()
        {
            if (!(bool) _memoryCache.Get(SERVICE_MEMORY))
            {
                _memoryCache.Set(SERVICE_MEMORY, true);
            }
            else
            {
                _memoryCache.Set(SERVICE_MEMORY, false);

                _memoryCache.Set(SERVICE_MEMORY, true);

                Start();
            }
        }

        /// <summary>
        ///     Processes the task by creating a client pipe to the core
        /// </summary>
        /// <param name="task">Requires a <see cref="string" /></param>
        private string ProcessTask(string task)
        {
            if (!IsValidJson(task)) return "";
            _loggerService.LogMessage(SERVICE_NAME, "Processing Task",
                LogLevel.Debug);
            _pipeClient = new PipeClient();
            _pipeClient.CreatePipe();
            _pipeClient.WriteString(task);
            _pipeClient.WaitForDrain();
            return _pipeClient.ReadString();
        }

        /// <summary>
        ///     Starts to run the server
        /// </summary>
        private void RunServer()
        {
            while (true)
            {
                //Find the TCP Service
                var service = _config.Components.Find(comp => comp.Name == "TCPService");
                //Create the TCP Listener to
                var server = new TcpListener(IPAddress.Parse(service.Properties.IP),
                    service.Properties.Port);

                server.Start();


                _loggerService.LogMessage(SERVICE_NAME,
                    $"Server has started on {service.Properties.IP}:{service.Properties.Port}.  Waiting for a connection...",
                    LogLevel.Info);
                _loggerService.LogMessage(SERVICE_NAME, "Started", LogLevel.Debug);

                var client = server.AcceptTcpClient();

                _loggerService.LogMessage(SERVICE_NAME, "Client Connected", LogLevel.Info);

                var stream = client.GetStream();
                while (!stream.DataAvailable)
                {
                }

                var bytes = new byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                var request = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(request))
                {
                    request = request.Substring(3, request.Length - 3);
                    if (!IsValidJson(request)) server.Stop();

                    //TODO: Check Request to make sure it is the JSON compatible and that it is able to convert to a task
                    ProcessTask(request);
                    _loggerService.LogMessage(SERVICE_NAME, _pipeClient.ReadString(),
                        LogLevel.Debug);

                    var response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols"
                                                          + Environment.NewLine
                                                          + "Connection: Upgrade" + Environment.NewLine
                                                          + "Upgrade: websocket" + Environment.NewLine
                                                          + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                                                              SHA1.Create().ComputeHash(
                                                                  Encoding.UTF8.GetBytes(
                                                                      new Regex("Sec-WebSocket-Key: (.*)")
                                                                          .Match(request)
                                                                          .Groups[1]
                                                                          .Value
                                                                          .Trim()
                                                                      + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                                                  )
                                                              )
                                                          ) + Environment.NewLine
                                                          + Environment.NewLine);

                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    if (!IsValidJson(request)) server.Stop();


                    _loggerService.LogMessage(SERVICE_NAME, "REQ: " + request,
                        LogLevel.Debug);
                    ProcessTask(request);
                    _loggerService.LogMessage(SERVICE_NAME, _pipeClient.ReadString(),
                        LogLevel.Debug);
                    var response = Encoding.UTF8.GetBytes("Data received");
                    stream.Write(response, 0, response.Length);
                }

                client.Close();
                server.Stop();
            }
        }

        /// <summary>
        ///     Checks if the string is a valid JSON
        /// </summary>
        /// <param name="strInput">Requires a <see cref="string" /></param>
        /// <returns>Returns a <see cref="bool" /></returns>
        private bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if (strInput.StartsWith("{") && strInput.EndsWith("}") || //For object
                strInput.StartsWith("[") && strInput.EndsWith("]")) //For array
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    _loggerService.LogMessage(SERVICE_NAME, jex.Message, LogLevel.Error);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }

            return false;
        }
    }
}