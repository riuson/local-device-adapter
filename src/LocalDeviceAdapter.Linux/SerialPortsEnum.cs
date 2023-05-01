using System.Collections.Generic;
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
            return new PortInfo[] { };
        }
    }
}
