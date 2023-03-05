using LocalDeviceAdapter.Handlers.Echo;
using System;
using System.ComponentModel;
using System.Net;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Server
{
    internal class ServerWorker : BackgroundWorker
    {
        private WebSocketServer server;

        protected override void Dispose(bool disposing)
        {
            server.Stop();
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            server = new WebSocketServer(IPAddress.Loopback, 4649, false);
            server.AddWebSocketService<EchoHandler>("/Echo");

            server.Start();

            if (server.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", server.Port);

                foreach (var path in server.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
        }
    }
}