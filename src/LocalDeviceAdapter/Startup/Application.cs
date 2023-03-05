using LocalDeviceAdapter.Server;
using System.Threading;

namespace LocalDeviceAdapter
{
    public class Application
    {
        private readonly IServer _server;

        public Application(IServer server)
        {
            _server = server;
        }

        public void Run()
        {
            _server.Start();

            Thread.Sleep(20000);

            _server.Shutdown();
        }
    }
}