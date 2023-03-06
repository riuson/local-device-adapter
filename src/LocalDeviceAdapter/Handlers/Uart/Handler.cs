using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Handler : IHandler
    {
        public (bool success, object answer) Process(RemoteCommand command)
        {
            switch (command.cmd)
            {
                case "list":
                {
                    var ports = GetSerialPorts();

                    return (true, ports);
                }
                default:
                    return (false, new object());
            }
        }

        private static object GetSerialPorts()
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
            return ports;
        }
    }
}