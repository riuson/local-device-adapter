using LocalDeviceAdapter.Handlers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Server
{
    internal class Server : IServer
    {
        private readonly IEnumerable<IHandlerInitializer> _handlerInitializers;
        private readonly IServerOptions _options;
        private WebSocketServer _server;
        private Task _serverTask;
        private CancellationTokenSource _tokenSource;

        public Server(
            IServerOptions options,
            IEnumerable<IHandlerInitializer> handlerInitializers)
        {
            _options = options;
            _handlerInitializers = handlerInitializers;
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _serverTask = new Task(
                () => Process(_tokenSource.Token),
                _tokenSource.Token,
                TaskCreationOptions.LongRunning);
            _serverTask.Start();
        }

        public void Shutdown()
        {
            _tokenSource.Cancel();
            _serverTask.Wait();
        }

        private void Process(CancellationToken token)
        {
            _server = new WebSocketServer(_options.IP, _options.LowerPort, false);

            foreach (var handlerInitializer in _handlerInitializers) handlerInitializer.Initialize(_server);

            _server.Start();

            if (_server.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", _server.Port);

                foreach (var path in _server.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }

            while (!token.IsCancellationRequested) token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));

            _server.Stop();

            foreach (var handlerInitializer in _handlerInitializers) handlerInitializer.DeInitialize(_server);
        }
    }
}