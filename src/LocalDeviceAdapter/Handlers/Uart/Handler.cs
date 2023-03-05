using System;
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
                try
                {
                    var command = JsonSerializer.Deserialize<RemoteCommand>(e.Data);
                    if (command is null)
                    {
                        Send("Invalid command");
                        return;
                    }

                    switch (command.cmd)
                    {
                        case "list":
                        {
                            var json = GetSerialPorts();
                            Send(json);
                            return;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Send("Invalid command");
                    return;
                }

            Send("Invalid command");
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
                    name = x.Name,
                    description = x.Description,
                    friendlyName = x.FriendlyName,
                    vendorId = getId(regexVid, x.HardwareId),
                    productId = getId(regexPid, x.HardwareId)
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