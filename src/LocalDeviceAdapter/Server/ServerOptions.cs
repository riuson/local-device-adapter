using System.Net;

namespace LocalDeviceAdapter.Server
{
    internal class ServerOptions : IServerOptions
    {
        public int LowerPort { get; set; } = 14690;
        public int UpperPort { get; set; } = 14699;
        public IPAddress IP { get; set; } = IPAddress.Loopback;
    }
}