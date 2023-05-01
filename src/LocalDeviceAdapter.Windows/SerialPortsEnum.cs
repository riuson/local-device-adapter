using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LocalDeviceAdapter.Common;

namespace LocalDeviceAdapter.PlatformSpecific
{
    /// <summary>
    /// Linux-specific code for Serial Ports enumeration.
    /// </summary>
    public static class SerialPortsEnum
    {
        /// <summary>
        /// Gets list of Serial Ports in JSON format.
        /// </summary>
        public static IEnumerable<PortInfo> GetPortsList()
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
                .Select(x => new PortInfo
                {
                    DeviceName = x.Name,
                    Description = x.Description,
                    FriendlyName = x.FriendlyName,
                    VendorId = getId(regexVid, x.HardwareId),
                    ProductId = getId(regexPid, x.HardwareId)
                })
                .ToArray();
            return ports;
        }
    }
}
