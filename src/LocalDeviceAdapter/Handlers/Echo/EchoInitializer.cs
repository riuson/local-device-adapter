using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Echo
{
    internal class EchoInitializer : IHandlerInitializer
    {
        public void Initialize(WebSocketServer server)
        {
            server.AddWebSocketService<EchoHandler>("/Echo");
        }

        public void DeInitialize(WebSocketServer server)
        {
            server.RemoveWebSocketService("/Echo");
        }
    }
}