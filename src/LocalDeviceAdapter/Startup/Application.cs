using System.Threading;
using LocalDeviceAdapter.Server;
using Microsoft.Extensions.Logging;

namespace LocalDeviceAdapter
{
    public class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly IServer _server;

        public Application(
            IServer server,
            ILogger<Application> logger)
        {
            this._server = server;
            this._logger = logger;
        }

        public void Run()
        {
            this._server.Start();
            this._logger.LogInformation("App started");

            Thread.Sleep(30000);

            this._server.Shutdown();
            this._logger.LogInformation("App stopped");
        }
    }
}
