using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            // SDRP_HARDWAREID: USB\VID_10C4&PID_EA60&REV_0100USB\VID_10C4&PID_EA60
            var regexVid = new Regex(@"(?<=VID_)[0-9a-fA-F]{4}");
            var regexPid = new Regex(@"(?<=PID_)[0-9a-fA-F]{4}");

            int getId(Regex regex, string value)
            {
                var match = regex.Match(value);

                if (!match.Success) return -1;

                if (int.TryParse(match.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
                    return result;

                return -1;
            }

            var ports = Win32SerialPortEnum.GetAllCOMPorts()
                .Select(x => new
                {
                    x.Name, x.Description,
                    x.FriendlyName,
                    VendorId = getId(regexVid, x.HardwareId),
                    ProductId = getId(regexPid, x.HardwareId)
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