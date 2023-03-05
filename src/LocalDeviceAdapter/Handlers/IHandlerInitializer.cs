using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers
{
    public interface IHandlerInitializer
    {
        void Initialize(WebSocketServer server);
        void DeInitialize(WebSocketServer server);
    }
}