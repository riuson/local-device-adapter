using WebSocketSharp;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Echo
{
    public class Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Send("Rsponse: " + e.Data);
        }
    }
}