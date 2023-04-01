using Microsoft.Extensions.Logging;

namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Initializer : IHandlerInitializer
    {
        private readonly ILogger<Handler> _logger;

        public Initializer(ILogger<Handler> logger)
        {
            _logger = logger;
        }

        public string Path => "/Info";

        public IHandler CreateHandler()
        {
            return new Handler(_logger);
        }
    }
}