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
            this._options = options;
            this._logger = logger;
            this._createWebServer = createWebServer;
        }

        public void Start()
        {
            try
            {
                this._server = this._createWebServer();
                this._serverTask = this._server.Listen(this._options.IP, this._options.LowerPort);
                this._logger.LogInformation($"Listening in port {4649}");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
            }
        }

        public void Shutdown()
        {
            try
            {
                this._server.Dispose();
                this._serverTask.Wait();
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
