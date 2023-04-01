using System;
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
            _server = server;
            _logger = logger;
        }

        public void Run()
        {
            _server.Start();
            _logger.LogInformation("App started");

            Thread.Sleep(30000);

            _server.Shutdown();
            _logger.LogInformation("App stopped");
        }
    }
}