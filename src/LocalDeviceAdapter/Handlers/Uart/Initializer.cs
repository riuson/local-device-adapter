using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Initializer : IHandlerInitializer
    {
        public void Initialize(WebSocketServer server)
        {
            server.AddWebSocketService<Handler>("/Uart");
        }

        public void DeInitialize(WebSocketServer server)
        {
            server.RemoveWebSocketService("/Uart");
        }
    }
}