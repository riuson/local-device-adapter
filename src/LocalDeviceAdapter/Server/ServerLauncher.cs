using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LocalDeviceAdapter.Server
{
    internal class ServerLauncher : IServer
    {
        private readonly Func<WebServer> _createWebServer;
        private readonly ILogger<ServerLauncher> _logger;
        private readonly IServerOptions _options;
        private WebServer _server;
        private Task _serverTask;

        public ServerLauncher(
            IServerOptions options,
            ILogger<ServerLauncher> logger,
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