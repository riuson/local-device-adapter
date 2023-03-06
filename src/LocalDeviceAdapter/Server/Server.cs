using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

//using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Server
{
    internal class Server : IServer
    {
        private readonly Func<WebServer> _createWebServer;
        private readonly ILogger<Server> _logger;
        private readonly IServerOptions _options;
        private WebServer _server;
        private Task _serverTask;

        //private WebSocketServer _server;

        public Server(
            IServerOptions options,
            ILogger<Server> logger,
            Func<WebServer> createWebServer)
        {
            _options = options;
            _logger = logger;
            _createWebServer = createWebServer;
        }

        public void Start()
        {
            try
            {
                _server = _createWebServer();
                _serverTask = _server.Listen(_options.IP, _options.LowerPort);
                _logger.LogInformation($"Listening in port {4649}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Shutdown()
        {
            try
            {
                _server.Dispose();
                _serverTask.Wait();
            }
            catch (AggregateException)
            {
                // do nothing. This will be thrown if the Listener has been stopped
            }
            catch (ObjectDisposedException)
            {
                // do nothing. This will be thrown if the Listener has been stopped
            }
        }
    }
}