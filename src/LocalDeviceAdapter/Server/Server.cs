using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalDeviceAdapter.Handlers;
using Microsoft.Extensions.Logging;
using Ninja.WebSockets;

//using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Server
{
    internal class Server : IServer
    {
        private readonly IEnumerable<IHandlerInitializer> _handlerInitializers;
        private readonly ILogger<Server> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServerOptions _options;
        private WebServer _server;

        //private WebSocketServer _server;
        private Task _serverTask;
        private CancellationTokenSource _tokenSource;

        public Server(
            IServerOptions options,
            IEnumerable<IHandlerInitializer> handlerInitializers,
            ILogger<Server> logger,
            ILoggerFactory loggerFactory)
        {
            _options = options;
            _handlerInitializers = handlerInitializers;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            var webSocketServerFactory = new WebSocketServerFactory();
            var supportedSubProtocols = new[] { "chatV1", "chatV2", "chatV3" };
            try
            {
                {
                    _server = new WebServer(webSocketServerFactory, _loggerFactory, supportedSubProtocols);
                    _server.Listen(4649);
                    _logger.LogInformation($"Listening in port {4649}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Shutdown()
        {
            _tokenSource.Cancel();
            _server.Dispose();
        }
    }
}