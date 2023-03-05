using System.Linq;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Handler : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.IsText)
                switch (e.Data)
                {
                    case "list":
                    {
                        var json = GetSerialPorts();
                        Send(json);
                        return;
                    }
                }

            Send("none");
        }

        private static string GetSerialPorts()
        {
            var ports = Win32SerialPortEnum.GetAllCOMPorts()
                .Select(x => new
                {
                    x.Name, x.Description
                })
                .ToArray();
            var json = JsonSerializer.Serialize(
                ports,
                new JsonSerializerOptions
                {
                    AllowTrailingCommas = false
                });
            return json;
        }
    }
}