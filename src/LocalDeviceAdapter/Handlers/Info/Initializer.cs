using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Info
{
    internal class Initializer : IHandlerInitializer
    {
        public void Initialize(WebSocketServer server)
        {
            server.AddWebSocketService<Handler>("/Info");
        }

        public void DeInitialize(WebSocketServer server)
        {
            server.RemoveWebSocketService("/Info");
        }
    }
}